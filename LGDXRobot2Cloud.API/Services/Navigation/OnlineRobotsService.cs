using LGDXRobot2Cloud.API.Services.Common;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Protos;
using LGDXRobot2Cloud.Utilities.Enums;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.API.Services.Navigation;

public class RobotCommandsEventArgs : EventArgs
{
  public Guid RobotId { get; set; }
  public required RobotClientsRobotCommands Commands { get; set; }
}

public interface IOnlineRobotsService
{
  Task AddRobotAsync(Guid robotId);
  Task RemoveRobotAsync(Guid robotId);
  Task UpdateRobotDataAsync(Guid robotId, RobotClientsExchange data);
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
    IMemoryCache memoryCache,
    IRobotService robotService
  ) : IOnlineRobotsService
{
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IRobotService _robotService = robotService ?? throw new ArgumentNullException(nameof(robotService));
  private static string GetOnlineRobotsKey(int realmId) => $"OnlineRobotsService_OnlineRobots_{realmId}";
  private static string GetRobotCommandsKey(Guid robotId) => $"OnlineRobotsService_RobotCommands_{robotId}";

  public event EventHandler<RobotCommandsEventArgs>? RobotCommandsChanged;

  protected virtual void OnRobotCommandsChanged(Guid robotId, RobotClientsRobotCommands commands)
  {
    RobotCommandsChanged?.Invoke(this, new RobotCommandsEventArgs { RobotId = robotId, Commands = commands });
  }

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
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId);
    if (realmId == null)
      return;

    var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey((int)realmId)) ?? [];
    OnlineRobotsIds.Add(robotId);
    // Register the robot
    _memoryCache.Set(GetOnlineRobotsKey((int)realmId), OnlineRobotsIds);
    _memoryCache.Set(GetRobotCommandsKey(robotId), new RobotClientsRobotCommands());
  }

  public async Task RemoveRobotAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId);
    if (realmId == null)
      return;

    // Unregister the robot
    var OnlineRobotsIds = _memoryCache.Get<HashSet<Guid>>(GetOnlineRobotsKey((int)realmId));
    if (OnlineRobotsIds != null && OnlineRobotsIds.Contains(robotId))
    {
      OnlineRobotsIds.Remove(robotId);
      _memoryCache.Set(GetOnlineRobotsKey((int)realmId), OnlineRobotsIds);
    }    
    _memoryCache.Remove(GetRobotCommandsKey(robotId));
  }

  public async Task UpdateRobotDataAsync(Guid robotId, RobotClientsExchange data)
  {
    if (_memoryCache.TryGetValue<bool>($"OnlineRobotsService_RobotData_Pause_{robotId}", out var _))
    {
      // Blocking too much data to rabbitmq
      return;
    }
    _memoryCache.Set($"OnlineRobotsService_RobotData_Pause_{robotId}", true, TimeSpan.FromSeconds(1));
    
    var robotStatus = ConvertRobotStatus(data.RobotStatus);
    await _bus.Publish(new RobotDataContract {
      RobotId = robotId,
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
      }
    });

    if(_memoryCache.TryGetValue<RobotClientsRobotCommands>(GetRobotCommandsKey(robotId), out var robotCommands))
    {
      if (robotCommands != null)
      {
        await _bus.Publish(new RobotCommandsContract {
          RobotId = robotId,
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
    await _bus.Publish(new RobotCommandsContract {
      RobotId = robotId,
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
    return _memoryCache.Get<RobotClientsRobotCommands>(GetRobotCommandsKey(robotId));
  }

  public async Task<bool> IsRobotOnlineAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId);
    if (realmId == null)
      return false;

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
      OnRobotCommandsChanged(robotId, robotCommands);
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
      OnRobotCommandsChanged(robotId, robotCommands);
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
      OnRobotCommandsChanged(robotId, robotCommands);
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