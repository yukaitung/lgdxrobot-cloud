using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface IOnlineRobotsService
{
  Task AddRobotAsync(Guid robotId);
  Task RemoveRobotAsync(Guid robotId);
  Task UpdateRobotDataAsync(Guid robotId, RobotClientsData data, bool realtime = false);

  bool SetAbortTask(Guid robotId);
  Task<bool> SetSoftwareEmergencyStopAsync(Guid robotId, bool enable);
  Task<bool> SetPauseTaskAssigementAsync(Guid robotId, bool enable);
  bool GetPauseAutoTaskAssignment(Guid robotId);

  IReadOnlyList<RobotClientsRobotCommands> GetRobotCommands(Guid robotId);
  IReadOnlyList<RobotClientsAutoTask> GetAutoTasks(Guid robotId);
}

public class OnlineRobotsService(
    IActivityLogService activityLogService,
    IBus bus,
    IEmailService emailService,
    IEventService eventService,
    IMemoryCache memoryCache,
    IRobotDataService robotDataService,
    IRobotService robotService
  ) : IOnlineRobotsService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly IEventService _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IRobotDataService _robotDataService = robotDataService ?? throw new ArgumentNullException(nameof(robotDataService));
  private readonly IRobotService _robotService = robotService ?? throw new ArgumentNullException(nameof(robotService));

  private static RobotStatus ConvertRobotStatus(RobotClientsRobotStatus robotStatus)
  {
    return robotStatus switch
    {
      RobotClientsRobotStatus.Idle => RobotStatus.Idle,
      RobotClientsRobotStatus.Running => RobotStatus.Running,
      RobotClientsRobotStatus.Stuck => RobotStatus.Stuck,
      RobotClientsRobotStatus.Aborting => RobotStatus.Aborting,
      RobotClientsRobotStatus.Paused => RobotStatus.Paused,
      RobotClientsRobotStatus.Critical => RobotStatus.Critical,
      RobotClientsRobotStatus.Charging => RobotStatus.Charging,
      RobotClientsRobotStatus.Offline => RobotStatus.Offline,
      _ => RobotStatus.Offline,
    };
  }

  public async Task AddRobotAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    _robotDataService.StartExchange(realmId, robotId);
  }

  public async Task RemoveRobotAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    _robotDataService.StopExchange(realmId, robotId);

    // Publish the robot is offline
    await _bus.Publish(new RobotDataContract
    {
      RobotId = robotId,
      RealmId = realmId,
      RobotStatus = RobotStatus.Offline
    });
  }

  public async Task UpdateRobotDataAsync(Guid robotId, RobotClientsData data, bool realtime = false)
  {
    if (_memoryCache.TryGetValue<bool>($"OnlineRobotsService_RobotData_Pause_{robotId}", out var _))
    {
      // Blocking too much data to rabbitmq
      return;
    }
    _memoryCache.Set($"OnlineRobotsService_RobotData_Pause_{robotId}", true, TimeSpan.FromMilliseconds(100));

    _robotDataService.SetRobotData(robotId, data);
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var robotStatus = ConvertRobotStatus(data.RobotStatus);
    var robotData = new RobotDataContract
    {
      RobotId = robotId,
      RealmId = realmId,
      RobotStatus = robotStatus,
      CriticalStatus = new RobotCriticalStatus
      {
        HardwareEmergencyStop = data.CriticalStatus.HardwareEmergencyStop,
        SoftwareEmergencyStop = data.CriticalStatus.SoftwareEmergencyStop,
        BatteryLow = [.. data.CriticalStatus.BatteryLow],
        MotorDamaged = [.. data.CriticalStatus.MotorDamaged]
      },
      Batteries = [.. data.Batteries],
      Position = new RobotDof
      {
        X = data.Position.X,
        Y = data.Position.Y,
        Rotation = data.Position.Rotation
      },
      NavProgress = new AutoTaskNavProgress
      {
        Eta = data.NavProgress.Eta,
        Recoveries = data.NavProgress.Recoveries,
        DistanceRemaining = data.NavProgress.DistanceRemaining,
        WaypointsRemaining = data.NavProgress.WaypointsRemaining,
        Plan = [.. data.NavProgress.Plan.Select(x => new Robot2Dof { X = x.X, Y = x.Y })]
      },
      CurrentTime = realtime ? DateTime.MaxValue : DateTime.UtcNow
    };
    await _bus.Publish(robotData);

    if (robotStatus == RobotStatus.Stuck)
    {
      if (!_memoryCache.TryGetValue<bool>($"OnlineRobotsService_RobotStuck_{robotId}", out var _))
      {
        // First stuck in 5 minutes, sending email
        await _emailService.SendRobotStuckEmailAsync(robotId, data.Position.X, data.Position.Y);
      }
      _memoryCache.Set($"OnlineRobotsService_RobotStuck_{robotId}", true, TimeSpan.FromMinutes(5));
    }
  }

  public bool SetAbortTask(Guid robotId)
  {
    if (_robotDataService.SetRobotCommands(robotId, new RobotClientsRobotCommands { AbortTask = true }))
    {
      _eventService.RobotCommandsHasUpdated(robotId);
      return true;
    }
    else
    {
      return false;
    }
  }

  public async Task<bool> SetSoftwareEmergencyStopAsync(Guid robotId, bool enable)
  {
    bool result = false;

    if (enable)
    {
      result = _robotDataService.SetRobotCommands(robotId, new RobotClientsRobotCommands { SoftwareEmergencyStopEnable = true });
    }
    else
    {
      result = _robotDataService.SetRobotCommands(robotId, new RobotClientsRobotCommands { SoftwareEmergencyStopDisable = true });
    }

    if (result)
    {
      _eventService.RobotCommandsHasUpdated(robotId);
      await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Robot),
        EntityId = robotId.ToString(),
        Action = enable ? ActivityAction.RobotSoftwareEmergencyStopEnabled : ActivityAction.RobotSoftwareEmergencyStopDisabled,
      });
    }
    return result;
  }

  public async Task<bool> SetPauseTaskAssigementAsync(Guid robotId, bool enable)
  {
    bool result = false;

    if (enable)
    {
      result = _robotDataService.SetRobotCommands(robotId, new RobotClientsRobotCommands { PauseTaskAssigementEnable = true });
    }
    else
    {
      result = _robotDataService.SetRobotCommands(robotId, new RobotClientsRobotCommands { PauseTaskAssigementDisable = true });
    }

    if (result)
    {
      _eventService.RobotCommandsHasUpdated(robotId);
      await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Robot),
        EntityId = robotId.ToString(),
        Action = enable ? ActivityAction.RobotPauseTaskAssignmentEnabled : ActivityAction.RobotPauseTaskAssignmentDisabled,
      });
    }
    return result;
  }

  public bool GetPauseAutoTaskAssignment(Guid robotId)
  {
    var robotData = _robotDataService.GetRobotData(robotId);
    if (robotData != null)
    {
      return robotData.RobotStatus == RobotClientsRobotStatus.Paused;
    }
    return false;
  }

  public IReadOnlyList<RobotClientsRobotCommands> GetRobotCommands(Guid robotId)
  {
    return _robotDataService.GetRobotCommands(robotId);
  }

  public IReadOnlyList<RobotClientsAutoTask> GetAutoTasks(Guid robotId)
  {
    return _robotDataService.GetAutoTasks(robotId);
  }
}