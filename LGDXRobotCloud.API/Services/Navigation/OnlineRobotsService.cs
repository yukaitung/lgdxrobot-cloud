using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.API.Services.Navigation;

public class RobotCommandsEventArgs : EventArgs
{
  public Guid RobotId { get; set; }
  public required RobotClientsRobotCommands Commands { get; set; }
}

public interface IOnlineRobotsService
{
  Task AddRobotAsync(Guid robotId);
  Task RemoveRobotAsync(Guid robotId);
  Task UpdateRobotDataAsync(Guid robotId, RobotClientsExchange data, bool realtime = false);
  RobotClientsRobotCommands? GetRobotCommands(Guid robotId);

  Task<bool> IsRobotOnlineAsync(Guid robotId);
  Task<bool> SetAbortTaskAsync(Guid robotId, bool enable);
  Task<bool> SetSoftwareEmergencyStopAsync(Guid robotId, bool enable);
  Task<bool> SetPauseTaskAssigementAsync(Guid robotId, bool enable);
  bool GetPauseAutoTaskAssignment(Guid robotId);

  void SetAutoTaskNextApi(Guid robotId, AutoTask task);
  AutoTask? GetAutoTaskNextApi(Guid robotId);
}

public class OnlineRobotsService(
    IBus bus,
    IEmailService emailService,
    IEventService eventService,
    IMemoryCache memoryCache,
    IRobotService robotService
  ) : IOnlineRobotsService
{
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly IEventService _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IRobotService _robotService = robotService ?? throw new ArgumentNullException(nameof(robotService));
  private static string GetOnlineRobotsKey(int realmId) => $"OnlineRobotsService_OnlineRobots_{realmId}";
  private static string GetRobotCommandsKey(Guid robotId) => $"OnlineRobotsService_RobotCommands_{robotId}";
  private const int robotAliveMinutes = 5;

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

  static private bool GenerateUnresolvableCriticalStatus(RobotClientsRobotCriticalStatus criticalStatus)
  {
    if (criticalStatus.HardwareEmergencyStop ||
        criticalStatus.BatteryLow.Count > 0 ||
        criticalStatus.MotorDamaged.Count > 0) 
    {
      return true;
    }
    return false;
  }

  public async Task AddRobotAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey(realmId)) ?? [];
    OnlineRobotsIds.Add(robotId);
    // Register the robot
    _memoryCache.Set(GetOnlineRobotsKey(realmId), OnlineRobotsIds);
    _memoryCache.Set(GetRobotCommandsKey(robotId), new RobotClientsRobotCommands());
    _memoryCache.Set($"OnlineRobotsService_RobotData_ConnectionAlive_{robotId}", true, TimeSpan.FromMinutes(robotAliveMinutes));
  }

  public async Task RemoveRobotAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    // Unregister the robot
    _memoryCache.TryGetValue(GetOnlineRobotsKey(realmId), out HashSet<Guid>? OnlineRobotsIds);
    if (OnlineRobotsIds != null && OnlineRobotsIds.Contains(robotId))
    {
      OnlineRobotsIds.Remove(robotId);
      _memoryCache.Set(GetOnlineRobotsKey(realmId), OnlineRobotsIds);
    }    
    _memoryCache.Remove(GetRobotCommandsKey(robotId));
    
    // Publish the robot is offline
    await _bus.Publish(new RobotDataContract
    {
      RobotId = robotId,
      RealmId = realmId,
      RobotStatus = RobotStatus.Offline,
      CurrentTime = DateTime.MinValue
    });
  }

  public async Task UpdateRobotDataAsync(Guid robotId, RobotClientsExchange data, bool realtime = false)
  {
    if (_memoryCache.TryGetValue<bool>($"OnlineRobotsService_RobotData_Pause_{robotId}", out var _))
    {
      // Blocking too much data to rabbitmq
      return;
    }
    _memoryCache.Set($"OnlineRobotsService_RobotData_Pause_{robotId}", true, TimeSpan.FromSeconds(1));
    _memoryCache.Set($"OnlineRobotsService_RobotData_ConnectionAlive_{robotId}", true, TimeSpan.FromMinutes(robotAliveMinutes));

    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var robotStatus = ConvertRobotStatus(data.RobotStatus);
    await _bus.Publish(new RobotDataContract {
      RobotId = robotId,
      RealmId = realmId,
      RobotStatus = robotStatus,
      CriticalStatus = new RobotCriticalStatus {
        HardwareEmergencyStop = data.CriticalStatus.HardwareEmergencyStop,
        SoftwareEmergencyStop = data.CriticalStatus.SoftwareEmergencyStop,
        BatteryLow = [.. data.CriticalStatus.BatteryLow],
        MotorDamaged = [.. data.CriticalStatus.MotorDamaged]
      },
      Batteries = [.. data.Batteries],
      Position = new RobotDof {
        X = data.Position.X,
        Y = data.Position.Y,
        Rotation = data.Position.Rotation
      },
      NavProgress = new AutoTaskNavProgress {
        Eta = data.NavProgress.Eta,
        Recoveries = data.NavProgress.Recoveries,
        DistanceRemaining = data.NavProgress.DistanceRemaining,
        WaypointsRemaining = data.NavProgress.WaypointsRemaining
      },
      CurrentTime = realtime ? DateTime.MaxValue : DateTime.UtcNow
    });

    if(_memoryCache.TryGetValue<RobotClientsRobotCommands>(GetRobotCommandsKey(robotId), out var robotCommands))
    {
      if (robotCommands != null)
      {
        await _bus.Publish(new RobotCommandsContract {
          RobotId = robotId,
          RealmId = realmId,
          Commands = new RobotCommands {
            AbortTask = robotCommands.AbortTask,
            PauseTaskAssigement = robotCommands.PauseTaskAssigement,
            SoftwareEmergencyStop = robotCommands.SoftwareEmergencyStop,
            RenewCertificate = robotCommands.RenewCertificate
          }
        });
      }
    }

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

  private async Task SetRobotCommandsAsync(Guid robotId, RobotClientsRobotCommands commands)
  {
    _memoryCache.Set(GetRobotCommandsKey(robotId), commands);
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    await _bus.Publish(new RobotCommandsContract {
      RobotId = robotId,
      RealmId = realmId,
      Commands = new RobotCommands {
        AbortTask = commands.AbortTask,
        PauseTaskAssigement = commands.PauseTaskAssigement,
        SoftwareEmergencyStop = commands.SoftwareEmergencyStop,
        RenewCertificate = commands.RenewCertificate
      }
    });
  }

  public RobotClientsRobotCommands? GetRobotCommands(Guid robotId)
  {
    _memoryCache.TryGetValue(GetRobotCommandsKey(robotId), out RobotClientsRobotCommands? robotCommands);
    return robotCommands;
  }

  public async Task<bool> IsRobotOnlineAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var onlineRobotsIds = _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey((int)realmId));
    return onlineRobotsIds != null && onlineRobotsIds.Contains(robotId);
  }

  public async Task<bool> SetAbortTaskAsync(Guid robotId, bool enable)
  {
    var robotCommands = GetRobotCommands(robotId);
    if (robotCommands != null)
    {
      robotCommands.AbortTask = enable;
      await SetRobotCommandsAsync(robotId, robotCommands);
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
    var robotCommands = GetRobotCommands(robotId);
    if (robotCommands != null)
    {
      robotCommands.SoftwareEmergencyStop = enable;
      await SetRobotCommandsAsync(robotId, robotCommands);
      _eventService.RobotCommandsHasUpdated(robotId);
      return true;
    }
    else 
    {
      return false;
    }
  }

  public async Task<bool> SetPauseTaskAssigementAsync(Guid robotId, bool enable)
  {
    var robotCommands = GetRobotCommands(robotId);
    if (robotCommands != null)
    {
      robotCommands.PauseTaskAssigement = enable;
      await SetRobotCommandsAsync(robotId, robotCommands);
      _eventService.RobotCommandsHasUpdated(robotId);
      return true;
    }
    else 
    {
      return false;
    }
  }

  public bool GetPauseAutoTaskAssignment(Guid robotId)
  {
    var robotCommands = GetRobotCommands(robotId);
    if (robotCommands != null) 
    {
      return robotCommands.PauseTaskAssigement;
    }
    else
    {
      return false;
    }
  }

  public void SetAutoTaskNextApi(Guid robotId, AutoTask autoTask)
  {
    _memoryCache.Set($"OnlineRobotsService_RobotHasNextTask_{robotId}", autoTask);
    _eventService.RobotHasNextTaskTriggered(robotId);
  }

  public AutoTask? GetAutoTaskNextApi(Guid robotId)
  {
    if (_memoryCache.TryGetValue($"OnlineRobotsService_RobotHasNextTask_{robotId}", out AutoTask? autoTask))
    {
      _memoryCache.Remove($"OnlineRobotsService_RobotHasNextTask_{robotId}");
      return autoTask;
    }
    else
    {
      return null;
    }
  }
}