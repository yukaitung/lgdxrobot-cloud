using LGDXRobotCloud.API.Repositories;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface IOnlineRobotsService
{
  Task AddRobotAsync(Guid robotId);
  Task RemoveRobotAsync(Guid robotId);
  Task UpdateRobotDataAsync(Guid robotId, RobotClientsData data);

  Task<bool> SetAbortTaskAsync(Guid robotId);
  Task<bool> SetSoftwareEmergencyStopAsync(Guid robotId, bool enable);
  Task<bool> SetPauseTaskAssignmentAsync(Guid robotId, bool enable);
  Task<bool> GetPauseAutoTaskAssignmentAsync(Guid robotId);
}

public class OnlineRobotsService(
    IActivityLogService activityLogService,
    IEmailService emailService,
    IMemoryCache memoryCache,
    IRobotDataRepository robotDataRepository,
    IRobotService robotService
  ) : IOnlineRobotsService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IRobotDataRepository _robotDataRepository = robotDataRepository ?? throw new ArgumentNullException(nameof(robotDataRepository));
  private readonly IRobotService _robotService = robotService ?? throw new ArgumentNullException(nameof(robotService));

  public async Task AddRobotAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    await _robotDataRepository.StartExchangeAsync(realmId, robotId);
  }

  public async Task RemoveRobotAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    await _robotDataRepository.StopExchangeAsync(realmId, robotId);
  }

  public async Task UpdateRobotDataAsync(Guid robotId, RobotClientsData data)
  {
    if (_memoryCache.TryGetValue<bool>($"OnlineRobotsService_RobotData_Pause_{robotId}", out var _))
    {
      // Blocking too much data to rabbitmq
      return;
    }
    _memoryCache.Set($"OnlineRobotsService_RobotData_Pause_{robotId}", true, TimeSpan.FromMilliseconds(100));

    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    await _robotDataRepository.SetRobotDataAsync(realmId, robotId, new RobotData
    {
      RobotStatus = (RobotStatus)data.RobotStatus,
      CriticalStatus = new RobotCriticalStatus
      {
        SoftwareEmergencyStop = data.CriticalStatus.SoftwareEmergencyStop,
        HardwareEmergencyStop = data.CriticalStatus.HardwareEmergencyStop,
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
      PauseTaskAssignment = data.PauseTaskAssignment
    });
    
    var robotStatus = (RobotStatus)data.RobotStatus;
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

  public async Task<bool> SetAbortTaskAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    if (await _robotDataRepository.AddRobotCommandAsync(realmId, robotId, new RobotClientsRobotCommands { AbortTask = true }))
    {
      return true;
    }
    else
    {
      return false;
    }
  }

  public async Task<bool> SetSoftwareEmergencyStopAsync(Guid robotId, bool enable)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    bool result = false;

    if (enable)
    {
      result = await _robotDataRepository.AddRobotCommandAsync(realmId, robotId, new RobotClientsRobotCommands { SoftwareEmergencyStopEnable = true });
    }
    else
    {
      result = await _robotDataRepository.AddRobotCommandAsync(realmId, robotId, new RobotClientsRobotCommands { SoftwareEmergencyStopDisable = true });
    }

    if (result)
    {
      await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Robot),
        EntityId = robotId.ToString(),
        Action = enable ? ActivityAction.RobotSoftwareEmergencyStopEnabled : ActivityAction.RobotSoftwareEmergencyStopDisabled,
      });
    }
    return result;
  }

  public async Task<bool> SetPauseTaskAssignmentAsync(Guid robotId, bool enable)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    bool result = false;

    if (enable)
    {
      result = await _robotDataRepository.AddRobotCommandAsync(realmId, robotId, new RobotClientsRobotCommands { PauseTaskAssignmentEnable = true });
    }
    else
    {
      result = await _robotDataRepository.AddRobotCommandAsync(realmId, robotId, new RobotClientsRobotCommands { PauseTaskAssignmentDisable = true });
    }

    if (result)
    {
      await _activityLogService.CreateActivityLogAsync(new ActivityLogCreateBusinessModel
      {
        EntityName = nameof(Robot),
        EntityId = robotId.ToString(),
        Action = enable ? ActivityAction.RobotPauseTaskAssignmentEnabled : ActivityAction.RobotPauseTaskAssignmentDisabled,
      });
    }
    return result;
  }

  public async Task<bool> GetPauseAutoTaskAssignmentAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var robotData = await _robotDataRepository.GetRobotDataAsync(realmId, robotId);
    if (robotData != null)
    {
      return robotData.PauseTaskAssignment;
    }
    return false;
  }
}