using Grpc.Core;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Constants;
using LGDXRobotCloud.Utilities.Enums;
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
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Authorisation;

namespace LGDXRobotCloud.API.Services;

[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.RobotClientsJwtScheme)]
public class RobotClientsService(
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IEventService eventService,
    IOnlineRobotsService OnlineRobotsService,
    IOptionsSnapshot<LgdxRobotCloudSecretConfiguration> lgdxRobotCloudSecretConfiguration,
    IRobotService robotService,
    ISlamService slamService
  ) : RobotClientsServiceBase
{
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService;
  private readonly IEventService _eventService = eventService;
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService;
  private readonly LgdxRobotCloudSecretConfiguration _lgdxRobotCloudSecretConfiguration = lgdxRobotCloudSecretConfiguration.Value;
  private readonly IRobotService _robotService = robotService;
  private readonly ISlamService _slamService = slamService;
  private Guid _streamingRobotId = Guid.Empty;
  private RobotClientsRobotStatus _streamingRobotStatus = RobotClientsRobotStatus.Offline;
  private readonly Channel<RobotClientsRespond> _streamMessageQueue = Channel.CreateUnbounded<RobotClientsRespond>();
  private readonly Channel<RobotClientsSlamRespond> _slamStreamMessageQueue = Channel.CreateUnbounded<RobotClientsSlamRespond>();

  private static Guid GetRobotId(ServerCallContext context)
  {
    var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
    return Guid.Parse(robotClaim!.Value);
  }

  [Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.RobotClientsCertificateScheme)]
  public override async Task<RobotClientsGreetRespond> Greet(RobotClientsGreet request, ServerCallContext context)
  {
    var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
    if (robotClaim == null || !Guid.TryParse(robotClaim.Value, out var robotId))
      return new RobotClientsGreetRespond
      {
        Status = RobotClientsResultStatus.Failed,
        AccessToken = string.Empty
      };

    var robotIdGuid = robotId;
    var robot = await _robotService.GetRobotAsync(robotIdGuid);
    if (robot == null)
      return new RobotClientsGreetRespond
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
        return new RobotClientsGreetRespond
        {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      if (robot.IsProtectingHardwareSerialNumber && incomingSystemInfo.McuSerialNumber != systemInfo.McuSerialNumber)
      {
        return new RobotClientsGreetRespond
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

    await _onlineRobotsService.AddRobotAsync(robotId);

    return new RobotClientsGreetRespond
    {
      Status = RobotClientsResultStatus.Success,
      AccessToken = token,
      IsRealtimeExchange = robot.IsRealtimeExchange,
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

  [RobotClientShouldOnline]
  public override async Task<RobotClientsRespond> Exchange(RobotClientsExchange request, ServerCallContext context)
  {
    var robotId = GetRobotId(context);

    await _onlineRobotsService.UpdateRobotDataAsync(robotId, request);
    var manualAutoTask = _onlineRobotsService.GetAutoTaskNextApi(robotId);
    if (manualAutoTask != null)
    {
      // Triggered by API
      return new RobotClientsRespond
      {
        Status = RobotClientsResultStatus.Success,
        Commands = _onlineRobotsService.GetRobotCommands(robotId),
        Task = await _autoTaskSchedulerService.AutoTaskNextConstructAsync(manualAutoTask)
      };
    }
    else
    {
      // Get AutoTask
      RobotClientsAutoTask? task = null;
      if (request.RobotStatus == RobotClientsRobotStatus.Idle)
      {
        task = await _autoTaskSchedulerService.GetAutoTaskAsync(robotId);
      }
      return new RobotClientsRespond
      {
        Status = RobotClientsResultStatus.Success,
        Commands = _onlineRobotsService.GetRobotCommands(robotId),
        Task = task
      };
    }
  }

  [RobotClientShouldOnline]
  public override async Task ExchangeStream(IAsyncStreamReader<RobotClientsExchange> requestStream, IServerStreamWriter<RobotClientsRespond> responseStream, ServerCallContext context)
  {
    var robotId = GetRobotId(context);

    _streamingRobotId = robotId;
    _eventService.RobotCommandsUpdated += OnRobotCommandsUpdated;
    _eventService.RobotHasNextTask += OnRobotHasNextTask;
    _eventService.AutoTaskCreated += OnAutoTaskCreated;

    var clientToServer = ExchangeStreamClientToServerAsync(robotId, requestStream, context);
    var serverToClient = ExchangeStreamServerToClientAsync(responseStream, context);
    await Task.WhenAll(clientToServer, serverToClient);
  }

  private async Task ExchangeStreamClientToServerAsync(Guid robotId, IAsyncStreamReader<RobotClientsExchange> requestStream, ServerCallContext context)
  {
    while (await requestStream.MoveNext(CancellationToken.None) && !context.CancellationToken.IsCancellationRequested)
    {
      var request = requestStream.Current;
      _streamingRobotStatus = request.RobotStatus;
      // Get auto task
      if (request.RobotStatus == RobotClientsRobotStatus.Idle)
      {
        var task = await _autoTaskSchedulerService.GetAutoTaskAsync(robotId);
        if (task != null)
        {
          await _streamMessageQueue.Writer.WriteAsync(new RobotClientsRespond
          {
            Status = RobotClientsResultStatus.Success,
            Commands = _onlineRobotsService.GetRobotCommands(_streamingRobotId),
            Task = task
          });
        }
      }
      await _onlineRobotsService.UpdateRobotDataAsync(robotId, request);
    }

    // The reading stream is completed, stop wirting task
    _eventService.RobotCommandsUpdated -= OnRobotCommandsUpdated;
    _eventService.RobotHasNextTask -= OnRobotHasNextTask;
    _eventService.AutoTaskCreated -= OnAutoTaskCreated;

    // Assume the robot going offline
    await _onlineRobotsService.RemoveRobotAsync(robotId);

    _streamMessageQueue.Writer.TryComplete();
    await _streamMessageQueue.Reader.Completion;
  }

  private async Task ExchangeStreamServerToClientAsync(IServerStreamWriter<RobotClientsRespond> responseStream, ServerCallContext context)
  {
    await foreach (var message in _streamMessageQueue.Reader.ReadAllAsync(context.CancellationToken))
    {
      await responseStream.WriteAsync(message);
    }
  }

  private async void OnRobotCommandsUpdated(object? sender, Guid robotId)
  {
    if (_streamingRobotId != robotId)
      return;
    await _streamMessageQueue.Writer.WriteAsync(new RobotClientsRespond
    {
      Status = RobotClientsResultStatus.Success,
      Commands = _onlineRobotsService.GetRobotCommands(_streamingRobotId),
    });
  }

  private async void OnAutoTaskCreated(object? sender, EventArgs e)
  {
    // Read
    if (_streamingRobotStatus != RobotClientsRobotStatus.Idle)
      return;
    var autoTask = await _autoTaskSchedulerService.GetAutoTaskAsync(_streamingRobotId);
    if (autoTask != null)
    {
      await _streamMessageQueue.Writer.WriteAsync(new RobotClientsRespond
      {
        Status = RobotClientsResultStatus.Success,
        Commands = _onlineRobotsService.GetRobotCommands(_streamingRobotId),
        Task = autoTask
      });
    }
  }

  private async void OnRobotHasNextTask(object? sender, Guid robotId)
  {
    // When an auto task is completed by API
    if (_streamingRobotId != robotId)
      return;
    var manualAutoTask = _onlineRobotsService.GetAutoTaskNextApi(robotId);
    RobotClientsAutoTask? task = null;
    if (manualAutoTask != null)
    {
      task = await _autoTaskSchedulerService.AutoTaskNextConstructAsync(manualAutoTask);
    }
    await _streamMessageQueue.Writer.WriteAsync(new RobotClientsRespond
    {
      Status = RobotClientsResultStatus.Success,
      Commands = _onlineRobotsService.GetRobotCommands(robotId),
      Task = task
    });
  }

  [RobotClientShouldOnline]
  public override async Task<RobotClientsRespond> AutoTaskNext(RobotClientsNextToken request, ServerCallContext context)
  {
    var robotId = GetRobotId(context);

    var task = await _autoTaskSchedulerService.AutoTaskNextAsync(robotId, request.TaskId, request.NextToken);
    return new RobotClientsRespond
    {
      Status = task != null ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Commands = _onlineRobotsService.GetRobotCommands(robotId),
      Task = task
    };
  }

  [RobotClientShouldOnline]
  public override async Task<RobotClientsRespond> AutoTaskAbort(RobotClientsAbortToken request, ServerCallContext context)
  {
    var robotId = GetRobotId(context);

    await _onlineRobotsService.SetAbortTaskAsync(robotId, false);

    AutoTaskAbortReason autoTaskAbortReason = (AutoTaskAbortReason)(int)request.AbortReason;
    var task = await _autoTaskSchedulerService.AutoTaskAbortAsync(robotId, request.TaskId, request.NextToken, autoTaskAbortReason);
    return new RobotClientsRespond
    {
      Status = task != null ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Commands = _onlineRobotsService.GetRobotCommands(robotId),
      Task = task
    };
  }

  [RobotClientShouldOnline]
  public override async Task SlamExchange(IAsyncStreamReader<RobotClientsSlamExchange> requestStream, IServerStreamWriter<RobotClientsSlamRespond> responseStream, ServerCallContext context)
  {
    var robotId = GetRobotId(context);

    _streamingRobotId = robotId;

    var clientToServer = SlamExchangeClientToServerAsync(robotId, requestStream, context);
    var serverToClient = SlamExchangeServerToClientAsync(responseStream, context);
    await Task.WhenAll(clientToServer, serverToClient);
  }

  private async Task SlamExchangeClientToServerAsync(Guid robotId, IAsyncStreamReader<RobotClientsSlamExchange> requestStream, ServerCallContext context)
  {
    while (await requestStream.MoveNext(CancellationToken.None) && !context.CancellationToken.IsCancellationRequested)
    {
      var request = requestStream.Current;
      await _slamService.UpdateMapDataAsync(robotId, request.Status, request.MapData);
      await _onlineRobotsService.UpdateRobotDataAsync(robotId, request.Exchange);
    }

    // The reading stream is completed, stop wirting task
    await _onlineRobotsService.RemoveRobotAsync(robotId);
    _slamStreamMessageQueue.Writer.TryComplete();
    await _slamStreamMessageQueue.Reader.Completion;
  }

  private async Task SlamExchangeServerToClientAsync(IServerStreamWriter<RobotClientsSlamRespond> responseStream, ServerCallContext context)
  {
    await responseStream.WriteAsync(new RobotClientsSlamRespond {});
    await foreach (var message in _slamStreamMessageQueue.Reader.ReadAllAsync(context.CancellationToken))
    {
      await responseStream.WriteAsync(message);
    }
  }
}