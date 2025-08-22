using Grpc.Core;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static LGDXRobotCloud.Protos.RobotClientsService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.API.Services.Automation;
using System.Threading.Channels;

namespace LGDXRobotCloud.API.Services;

[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.RobotClientsJwtScheme)]
public class RobotClientsService(
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IOnlineRobotsService OnlineRobotsService,
    IOptionsSnapshot<LgdxRobotCloudSecretConfiguration> lgdxRobotCloudSecretConfiguration,
    IRobotService robotService,
    ISlamService slamService
  ) : RobotClientsServiceBase
{
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService;
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService;
  private readonly LgdxRobotCloudSecretConfiguration _lgdxRobotCloudSecretConfiguration = lgdxRobotCloudSecretConfiguration.Value;
  private readonly IRobotService _robotService = robotService;
  private readonly ISlamService _slamService = slamService;

  private bool pauseAutoTaskAssignmentEffective = false;
  private Guid _streamingRobotId = Guid.Empty;
  private readonly Channel<RobotClientsResponse> _streamMessageQueue = Channel.CreateUnbounded<RobotClientsResponse>();
  private readonly Channel<RobotClientsSlamCommands> _slamStreamMessageQueue = Channel.CreateUnbounded<RobotClientsSlamCommands>();

  private static Guid GetRobotId(ServerCallContext context)
  {
    var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
    return Guid.Parse(robotClaim!.Value);
  }

  [Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.RobotClientsCertificateScheme)]
  public override async Task<RobotClientsGreetResponse> Greet(RobotClientsGreet request, ServerCallContext context)
  {
    var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
    if (robotClaim == null || !Guid.TryParse(robotClaim.Value, out var robotId))
      return new RobotClientsGreetResponse
      {
        Status = RobotClientsResultStatus.Failed,
        AccessToken = string.Empty
      };

    var robotIdGuid = robotId;
    var robot = await _robotService.GetRobotAsync(robotIdGuid);
    if (robot == null)
      return new RobotClientsGreetResponse
      {
        Status = RobotClientsResultStatus.Failed,
        AccessToken = string.Empty
      };

    // Compare System Info
    var incomingSystemInfo = new RobotSystemInfoBusinessModel
    {
      Id = 0,
      Cpu = request.SystemInfo.Cpu,
      IsLittleEndian = request.SystemInfo.IsLittleEndian,
      Motherboard = request.SystemInfo.Motherboard,
      MotherboardSerialNumber = request.SystemInfo.MotherboardSerialNumber,
      RamMiB = request.SystemInfo.RamMiB,
      Gpu = request.SystemInfo.Gpu,
      Os = request.SystemInfo.Os,
      Is32Bit = request.SystemInfo.Is32Bit,
      McuSerialNumber = request.SystemInfo.McuSerialNumber,
    };
    var systemInfo = robot.RobotSystemInfo;
    if (systemInfo == null)
    {
      // Create Robot System Info for the first time
      await _robotService.CreateRobotSystemInfoAsync(robotIdGuid, incomingSystemInfo.ToCreateBusinessModel());
    }
    else
    {
      // Hardware Protection
      if (robot.IsProtectingHardwareSerialNumber && incomingSystemInfo.MotherboardSerialNumber != systemInfo.MotherboardSerialNumber)
      {
        return new RobotClientsGreetResponse
        {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      if (robot.IsProtectingHardwareSerialNumber && incomingSystemInfo.McuSerialNumber != systemInfo.McuSerialNumber)
      {
        return new RobotClientsGreetResponse
        {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      await _robotService.UpdateRobotSystemInfoAsync(robotIdGuid, incomingSystemInfo.ToUpdateBusinessModel());
    }

    var chassisInfo = await _robotService.GetRobotChassisInfoAsync(robotIdGuid);

    // Generate Access Token
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_lgdxRobotCloudSecretConfiguration.RobotClientsJwtSecret));
    var credentials = new SigningCredentials(securityKey, _lgdxRobotCloudSecretConfiguration.RobotClientsJwtAlgorithm);
    var secToken = new JwtSecurityToken(
      _lgdxRobotCloudSecretConfiguration.RobotClientsJwtIssuer,
      _lgdxRobotCloudSecretConfiguration.RobotClientsJwtIssuer,
      [new Claim(ClaimTypes.NameIdentifier, robot.Id.ToString())],
      DateTime.UtcNow,
      DateTime.UtcNow.AddMinutes(_lgdxRobotCloudSecretConfiguration.RobotClientsJwtExpireMins),
      credentials);
    var token = new JwtSecurityTokenHandler().WriteToken(secToken);

    return new RobotClientsGreetResponse
    {
      Status = RobotClientsResultStatus.Success,
      AccessToken = token,
      ChassisInfo = new RobotClientsChassisInfo
      {
        RobotTypeId = chassisInfo!.RobotTypeId,
        ChassisLX = chassisInfo.ChassisLengthX,
        ChassisLY = chassisInfo.ChassisLengthY,
        ChassisWheelCount = chassisInfo.ChassisWheelCount,
        ChassisWheelRadius = chassisInfo.ChassisWheelRadius,
        BatteryCount = chassisInfo.BatteryCount,
        BatteryMaxVoltage = chassisInfo.BatteryMaxVoltage,
        BatteryMinVoltage = chassisInfo.BatteryMinVoltage,
      }
    };
  }

  /*
   * Exchange
   */
  public override async Task Exchange(IAsyncStreamReader<RobotClientsExchange> requestStream, IServerStreamWriter<RobotClientsResponse> responseStream, ServerCallContext context)
  {
    var robotId = GetRobotId(context);

    _streamingRobotId = robotId;

    var clientToServer = ExchangeStreamClientToServerAsync(robotId, requestStream, context);
    var serverToClient = ExchangeStreamServerToClientAsync(responseStream, context);
    await Task.WhenAll(clientToServer, serverToClient);
  }

  private async Task ExchangeStreamClientToServerAsync(Guid robotId, IAsyncStreamReader<RobotClientsExchange> requestStream, ServerCallContext context)
  {
    try
    {
      await _onlineRobotsService.AddRobotAsync(robotId);
      await _autoTaskSchedulerService.RunSchedulerRobotNewJoinAsync(robotId);
      while (await requestStream.MoveNext(CancellationToken.None) && !context.CancellationToken.IsCancellationRequested)
      {
        // Process Data
        var request = requestStream.Current;
        await _onlineRobotsService.UpdateRobotDataAsync(robotId, request.RobotData);
        if (request.NextToken != null && request.NextToken.NextToken.Length > 0)
        {
          await _autoTaskSchedulerService.AutoTaskNextAsync(robotId, request.NextToken.TaskId, request.NextToken.NextToken);
        }
        if (request.AbortToken != null && request.AbortToken.NextToken.Length > 0)
        {
          await _autoTaskSchedulerService.AutoTaskAbortAsync(robotId, request.AbortToken.TaskId, request.AbortToken.NextToken,
            (Utilities.Enums.AutoTaskAbortReason)(int)request.AbortToken.AbortReason);
        }
        // Process Pause Auto Task Assignment
        if (pauseAutoTaskAssignmentEffective)
        {
          if (request.RobotData.RobotStatus != RobotClientsRobotStatus.Paused &&
                !request.RobotData.PauseTaskAssignment)
          {
            // Exit pause auto task assignment mode and request a task
            pauseAutoTaskAssignmentEffective = false;
            await _autoTaskSchedulerService.RunSchedulerRobotReadyAsync(robotId);
          }
        }
        else if (request.RobotData.RobotStatus == RobotClientsRobotStatus.Paused &&
                  request.RobotData.PauseTaskAssignment)
        {
          // Enter pause auto task assignment mode
          pauseAutoTaskAssignmentEffective = true;
        }
      }
    }
    finally
    {
      // The reading stream is completed, stop wirting task
      await _onlineRobotsService.RemoveRobotAsync(robotId);
    }
    
    _streamMessageQueue.Writer.TryComplete();
    await _streamMessageQueue.Reader.Completion;
  }

  private async Task ExchangeStreamServerToClientAsync(IServerStreamWriter<RobotClientsResponse> responseStream, ServerCallContext context)
  {
    await responseStream.WriteAsync(new RobotClientsResponse());
    await foreach (var message in _streamMessageQueue.Reader.ReadAllAsync(context.CancellationToken))
    {
      await responseStream.WriteAsync(message);
    }
  }
  
  /*
   * SlamExchange
   */
  public override async Task SlamExchange(IAsyncStreamReader<RobotClientsSlamExchange> requestStream, IServerStreamWriter<RobotClientsSlamCommands> responseStream, ServerCallContext context)
  {
    var robotId = GetRobotId(context);
    _streamingRobotId = robotId;

    var clientToServer = SlamExchangeClientToServerAsync(robotId, requestStream, context);
    var serverToClient = SlamExchangeServerToClientAsync(responseStream, context);
    await Task.WhenAll(clientToServer, serverToClient);
  }

  private async Task SlamExchangeClientToServerAsync(Guid robotId, IAsyncStreamReader<RobotClientsSlamExchange> requestStream, ServerCallContext context)
  {
    try
    {
      if (await _slamService.StartSlamAsync(robotId))
      {
        // Only one robot can running SLAM at a time in a realm
        // The second robot will be ternimated
        while (await requestStream.MoveNext(CancellationToken.None) && !context.CancellationToken.IsCancellationRequested)
        {
          var request = requestStream.Current;
          await _slamService.UpdateSlamDataAsync(robotId, request.Status, request.MapData);
          await _onlineRobotsService.UpdateRobotDataAsync(robotId, request.RobotData);
        }
      }
    }
    finally
    {
      // The reading stream is completed, stop wirting task
      await _slamService.StopSlamAsync(robotId);
      _slamStreamMessageQueue.Writer.TryComplete();
    }

    await _slamStreamMessageQueue.Reader.Completion;
  }

  private async Task SlamExchangeServerToClientAsync(IServerStreamWriter<RobotClientsSlamCommands> responseStream, ServerCallContext context)
  {
    await responseStream.WriteAsync(new RobotClientsSlamCommands());
    await foreach (var message in _slamStreamMessageQueue.Reader.ReadAllAsync(context.CancellationToken))
    {
      await responseStream.WriteAsync(message);
    }
  }
}