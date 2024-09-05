using LGDXRobot2Cloud.Protos;
using Microsoft.Extensions.Caching.Memory;

// Not expected to run in distributed cache mode, maybe group the robot

namespace LGDXRobot2Cloud.API.Services;

public record RobotDataComposite
{
  public required RobotClientsRobotCommand Commands { get; set; }
  public required RobotClientsExchange Data { get; set; }
  public bool UnresolvableCriticalStatus { get; set; } = false;
}

public interface IOnlineRobotsService
{
  void AddRobot(Guid robotId);
  void RemoveRobot(Guid robotId);
  void SetRobotData(Guid robotId, RobotClientsExchange data);
  Dictionary<Guid, RobotDataComposite> GetRobotData(Guid robotId);
  Dictionary<Guid, RobotDataComposite> GetRobotsData(List<Guid> robotIds);
  RobotClientsRobotCommand? GetRobotCommands(Guid robotId);

  bool IsRobotOnline(Guid robotId);
  bool UpdateSoftwareEmergencyStop(Guid robotId, bool enable);
  bool UpdatePauseTaskAssigement(Guid robotId, bool enable);
  bool GetPauseAutoTaskAssignment(Guid robotId);
}

public class OnlineRobotsService(IMemoryCache memoryCache) : IOnlineRobotsService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
  private readonly string OnlineRobotssKey = "OnlineRobotsService_OnlineRobotss";

  static private bool GenerateUnresolvableCriticalStatus(RobotClientsRobotCriticalStatus criticalStatus)
  {
    if (criticalStatus.HardwareEmergencyStopEnabled ||
        criticalStatus.BatteryLow.Count > 0 ||
        criticalStatus.MotorDamaged.Count > 0) 
        {
          return true;
        }
    return false;
  }

  public void AddRobot(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(OnlineRobotssKey, out var OnlineRobotsIds);
    OnlineRobotsIds ??= [];
    OnlineRobotsIds.Add(robotId);
    _memoryCache.Set(OnlineRobotssKey, OnlineRobotsIds);
    _memoryCache.Set($"OnlineRobotsService_RobotData_{robotId}", new RobotDataComposite{
      Commands = new RobotClientsRobotCommand(),
      Data = new RobotClientsExchange()
    });
  }

  public void RemoveRobot(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(OnlineRobotssKey, out var OnlineRobotsIds);
    if (OnlineRobotsIds != null && OnlineRobotsIds.Contains(robotId))
    {
      OnlineRobotsIds.Remove(robotId);
      _memoryCache.Set(OnlineRobotssKey, OnlineRobotsIds);
    }    
    _memoryCache.Remove($"OnlineRobotsService_RobotData_{robotId}");
  }

  public void SetRobotData(Guid robotId, RobotClientsExchange data)
  {
    _memoryCache.TryGetValue<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}", out var robotDataComposite);
    if (robotDataComposite != null)
    {
      robotDataComposite.Data = data;
      robotDataComposite.UnresolvableCriticalStatus = GenerateUnresolvableCriticalStatus(robotDataComposite.Data.CriticalStatus);
      _memoryCache.Set($"OnlineRobotsService_RobotData_{robotId}", robotDataComposite);
    }
  }

  public Dictionary<Guid, RobotDataComposite> GetRobotData(Guid robotId)
  {
    return GetRobotsData([robotId]);
  }

  public Dictionary<Guid, RobotDataComposite> GetRobotsData(List<Guid> robotIds)
  {
    Dictionary<Guid, RobotDataComposite> result = [];
    foreach (var robotId in robotIds)
    {
      _memoryCache.TryGetValue<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}", out var robotDataComposite);
      if (robotDataComposite != null)
      {
        result[robotId] = robotDataComposite;
      }
    }
    return result;
  }

  public RobotClientsRobotCommand? GetRobotCommands(Guid robotId)
  {
    _memoryCache.TryGetValue<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}", out var robotDataComposite);
    if (robotDataComposite != null)
    {
      return robotDataComposite.Commands;
    }
    return null;
  }

  public bool IsRobotOnline(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(OnlineRobotssKey, out var OnlineRobotsIds);
    return OnlineRobotsIds != null && OnlineRobotsIds.Contains(robotId);
  }

  public bool UpdateSoftwareEmergencyStop(Guid robotId, bool enable)
  {
    _memoryCache.TryGetValue<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}", out var robotDataComposite);
    if (robotDataComposite != null) 
    {
      robotDataComposite.Commands.SoftwareEmergencyStop = enable;
      _memoryCache.Set($"OnlineRobotsService_RobotData_{robotId}", robotDataComposite);
      return true;
    }
    else 
    {
      return false;
    }
  }

  public bool UpdatePauseTaskAssigement(Guid robotId, bool enable)
  {
    _memoryCache.TryGetValue<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}", out var robotDataComposite);
    if (robotDataComposite != null) 
    {
      robotDataComposite.Commands.PauseTaskAssigement = enable;
      _memoryCache.Set($"OnlineRobotsService_RobotData_{robotId}", robotDataComposite);
      return true;
    }
    else 
    {
      return false;
    }
  }

  public bool GetPauseAutoTaskAssignment(Guid robotId)
  {
    _memoryCache.TryGetValue<RobotDataComposite>($"OnlineRobotsService_RobotData_{robotId}", out var robotDataComposite);
    if (robotDataComposite != null) 
    {
      return robotDataComposite.Commands.PauseTaskAssigement;
    }
    return false;
  }
}