using LGDXRobotCloud.API.Repositories;
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
  Task UpdateRobotDataAsync(Guid robotId, RobotClientsData data);

  Task<bool> SetAbortTaskAsync(Guid robotId);
  Task<bool> SetSoftwareEmergencyStopAsync(Guid robotId, bool enable);
  Task<bool> SetPauseTaskAssignmentAsync(Guid robotId, bool enable);
  Task<bool> GetPauseAutoTaskAssignmentAsync(Guid robotId);
}

public class OnlineRobotsService(
    IActivityLogService activityLogService,
    IBus bus,
    IEmailService emailService,
    IMemoryCache memoryCache,
    IRobotDataRepository robotDataRepository,
    IRobotService robotService
  ) : IOnlineRobotsService
{
  private readonly IActivityLogService _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly IRobotDataRepository _robotDataRepository = robotDataRepository ?? throw new ArgumentNullException(nameof(robotDataRepository));
  private readonly IRobotService _robotService = robotService ?? throw new ArgumentNullException(nameof(robotService));

  private RobotDataContract sendRobotData = new();

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
    await _robotDataRepository.StartExchangeAsync(realmId, robotId);
  }

  public async Task RemoveRobotAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    await _robotDataRepository.StopExchangeAsync(realmId, robotId);

    // Publish the robot is offline
    await _bus.Publish(new RobotDataContract
    {
      RobotId = robotId,
      RealmId = realmId,
      RobotStatus = RobotStatus.Offline
    });
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
    await _robotDataRepository.SetRobotDataAsync(realmId, robotId, data);
    var robotStatus = ConvertRobotStatus(data.RobotStatus);

    // Update the robot data
    sendRobotData.RobotId = robotId;
    sendRobotData.RealmId = realmId;
    sendRobotData.RobotStatus = robotStatus;
    sendRobotData.CriticalStatus.HardwareEmergencyStop = data.CriticalStatus.HardwareEmergencyStop;
    sendRobotData.CriticalStatus.SoftwareEmergencyStop = data.CriticalStatus.SoftwareEmergencyStop;
    sendRobotData.CriticalStatus.BatteryLow = [.. data.CriticalStatus.BatteryLow];
    sendRobotData.CriticalStatus.MotorDamaged = [.. data.CriticalStatus.MotorDamaged];
    sendRobotData.Batteries = [.. data.Batteries];
    sendRobotData.Position.X = data.Position.X;
    sendRobotData.Position.Y = data.Position.Y;
    sendRobotData.Position.Rotation = data.Position.Rotation;
    sendRobotData.NavProgress.Eta = data.NavProgress.Eta;
    sendRobotData.NavProgress.Recoveries = data.NavProgress.Recoveries;
    sendRobotData.NavProgress.DistanceRemaining = data.NavProgress.DistanceRemaining;
    sendRobotData.NavProgress.WaypointsRemaining = data.NavProgress.WaypointsRemaining;
    sendRobotData.NavProgress.Plan = [.. data.NavProgress.Plan.Select(x => new Robot2Dof { X = x.X, Y = x.Y })];
    sendRobotData.PauseTaskAssignment = data.PauseTaskAssignment;
    await _bus.Publish(sendRobotData);

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