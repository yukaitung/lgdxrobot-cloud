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

namespace LGDXRobotCloud.API.Services;

[Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.RobotClientsJwtScheme)]
public class RobotClientsService(
    IAutoTaskSchedulerService autoTaskSchedulerService,
    IEventService eventService,
    IOnlineRobotsService OnlineRobotsService,
    IOptionsSnapshot<LgdxRobotCloudSecretConfiguration> lgdxRobotCloudSecretConfiguration,
    IRobotService robotService
  ) : RobotClientsServiceBase
{
  private readonly IAutoTaskSchedulerService _autoTaskSchedulerService = autoTaskSchedulerService;
  private readonly IEventService _eventService = eventService;
  private readonly IOnlineRobotsService _onlineRobotsService = OnlineRobotsService;
  private readonly LgdxRobotCloudSecretConfiguration _lgdxRobotCloudSecretConfiguration = lgdxRobotCloudSecretConfiguration.Value;
  private readonly IRobotService _robotService = robotService;
  private Guid _streamingRobotId = Guid.Empty;
  private RobotClientsRobotStatus _streamingRobotStatus = RobotClientsRobotStatus.Offline;
  private readonly Channel<RobotClientsRespond> _streamMessageQueue = Channel.CreateUnbounded<RobotClientsRespond>();

  private static Guid? ValidateRobotClaim(ServerCallContext context)
  {
    var robotClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
    if (robotClaim == null)
    {
      return null;
    }
    return Guid.Parse(robotClaim.Value);
  }

  private static RobotClientsRespond ValidateRobotClaimFailed()
  {
    return new RobotClientsRespond {
      Status = RobotClientsResultStatus.Failed,
    };
  }

  // TODO: Validate in authorisation
  private async Task<bool> ValidateOnlineRobotsAsync(Guid robotId)
  {
    return await _onlineRobotsService.IsRobotOnlineAsync(robotId);
  }

  private static RobotClientsRespond ValidateOnlineRobotsFailed()
  {
    return new RobotClientsRespond {
      Status = RobotClientsResultStatus.Failed,
    };
  }

  [Authorize(AuthenticationSchemes = LgdxRobotCloudAuthenticationSchemes.RobotClientsCertificateScheme)]
  public override async Task<RobotClientsGreetRespond> Greet(RobotClientsGreet request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return new RobotClientsGreetRespond {
        Status = RobotClientsResultStatus.Failed,
        AccessToken = string.Empty
      };

    var robotIdGuid = (Guid)robotId;
    var robot = await _robotService.GetRobotAsync(robotIdGuid);
    if (robot == null)
      return new RobotClientsGreetRespond {
        Status = RobotClientsResultStatus.Failed,
        AccessToken = string.Empty
      };

    // Compare System Info
    var incomingSystemInfo = new RobotSystemInfoBusinessModel {
      Id = 0,
      Cpu = request.SystemInfo.Cpu,
      IsLittleEndian =  request.SystemInfo.IsLittleEndian,
      Motherboard = request.SystemInfo.Motherboard,
      MotherboardSerialNumber = request.SystemInfo.MotherboardSerialNumber,
      RamMiB =  request.SystemInfo.RamMiB,
      Gpu =  request.SystemInfo.Gpu,
      Os =  request.SystemInfo.Os,
      Is32Bit = request.SystemInfo.Is32Bit,
      McuSerialNumber = request.SystemInfo.McuSerialNumber,
    };
    var systemInfo = await _robotService.GetRobotSystemInfoAsync(robotIdGuid);
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
        return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      if (robot.IsProtectingHardwareSerialNumber && incomingSystemInfo.McuSerialNumber != systemInfo.McuSerialNumber)
      {
        return new RobotClientsGreetRespond {
          Status = RobotClientsResultStatus.Failed,
          AccessToken = string.Empty
        };
      }
      await _robotService.UpdateRobotSystemInfoAsync(robotIdGuid, incomingSystemInfo.ToUpdateBusinessModel());
    }

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

    await _onlineRobotsService.AddRobotAsync((Guid)robotId);

    return new RobotClientsGreetRespond {
      Status = RobotClientsResultStatus.Success,
      AccessToken = token,
      IsRealtimeExchange = await _robotService.GetRobotIsRealtimeExchange((Guid)robotId)
    };
  }

  public override async Task<RobotClientsRespond> Exchange(RobotClientsExchange request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!await ValidateOnlineRobotsAsync((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    await _onlineRobotsService.UpdateRobotDataAsync((Guid)robotId, request);
    var manualAutoTask = _onlineRobotsService.GetAutoTaskNextApi((Guid)robotId);
    if (manualAutoTask != null)
    {
      // Triggered by API
      return new RobotClientsRespond {
        Status = RobotClientsResultStatus.Success,
        Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
        Task = await _autoTaskSchedulerService.AutoTaskNextConstructAsync(manualAutoTask)
      };
    }
    else
    {
      // Get AutoTask
      RobotClientsAutoTask? task = null;
      if (request.RobotStatus == RobotClientsRobotStatus.Idle)
      {
        task = await _autoTaskSchedulerService.GetAutoTaskAsync((Guid)robotId);
      }
      return new RobotClientsRespond {
        Status = RobotClientsResultStatus.Success,
        Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
        Task = task
      };
    }
  }

  public override async Task ExchangeStream(IAsyncStreamReader<RobotClientsExchange> requestStream, IServerStreamWriter<RobotClientsRespond> responseStream, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
    {
      var response = ValidateRobotClaimFailed();
      await responseStream.WriteAsync(response);
      return;
    }
    if (!await ValidateOnlineRobotsAsync((Guid)robotId))
    {
      var response = ValidateOnlineRobotsFailed();
      await responseStream.WriteAsync(response);
      return;
    }
    _streamingRobotId = (Guid)robotId;
    _eventService.RobotCommandsUpdated += OnRobotCommandsUpdated;
    _eventService.RobotHasNextTask += OnRobotHasNextTask;
    _eventService.AutoTaskCreated += OnAutoTaskCreated;

    var clientToServer = ExchangeStreamClientToServerAsync((Guid)robotId, requestStream, context);
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
          await _streamMessageQueue.Writer.WriteAsync(new RobotClientsRespond {
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
    await _streamMessageQueue.Writer.WriteAsync(new RobotClientsRespond {
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
      await _streamMessageQueue.Writer.WriteAsync(new RobotClientsRespond {
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
    await _streamMessageQueue.Writer.WriteAsync(new RobotClientsRespond {
      Status = RobotClientsResultStatus.Success,
      Commands = _onlineRobotsService.GetRobotCommands(robotId),
      Task = task
    });
  }

  public override async Task<RobotClientsRespond> AutoTaskNext(RobotClientsNextToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!await ValidateOnlineRobotsAsync((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    var task = await _autoTaskSchedulerService.AutoTaskNextAsync((Guid)robotId, request.TaskId, request.NextToken);
    return new RobotClientsRespond {
      Status = task != null ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
      Task = task
    };
  }

  public override async Task<RobotClientsRespond> AutoTaskAbort(RobotClientsAbortToken request, ServerCallContext context)
  {
    var robotId = ValidateRobotClaim(context);
    if (robotId == null)
      return ValidateRobotClaimFailed();
    if (!await ValidateOnlineRobotsAsync((Guid)robotId))
      return ValidateOnlineRobotsFailed();

    await _onlineRobotsService.SetAbortTaskAsync((Guid)robotId, false);

    AutoTaskAbortReason autoTaskAbortReason = (AutoTaskAbortReason)(int)request.AbortReason;
    var task = await _autoTaskSchedulerService.AutoTaskAbortAsync((Guid)robotId, request.TaskId, request.NextToken, autoTaskAbortReason);
    return new RobotClientsRespond {
      Status = task != null ? RobotClientsResultStatus.Success : RobotClientsResultStatus.Failed,
      Commands = _onlineRobotsService.GetRobotCommands((Guid)robotId),
      Task = task
    };
  }
}