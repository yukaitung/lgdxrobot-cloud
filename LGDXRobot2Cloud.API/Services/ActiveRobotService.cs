using System.Collections.ObjectModel;
using LGDXRobot2Cloud.Protos;
using Microsoft.Extensions.Caching.Memory;

namespace LGDXRobot2Cloud.API.Services;

public record RobotDataBrief 
{
  public RobotClientsRobotStatus RobotStatus { get; set; }
  public required IEnumerable<double> Batteries { get; set; }
  public required RobotClientsDof Position { get; set; }
}

public interface IActiveRobotService
{
  void AddRobot(Guid robotId);
  void RemoveRobot(Guid robotId);
  void SetRobotData(Guid robotId, RobotClientsExchange data);
  ReadOnlyDictionary<Guid, RobotDataBrief>? GetRobotsDataBrief();
  //HashSet<Guid> GetActiveRobots();
  bool IsRobotActive(Guid robotId);
}

public class ActiveRobotService(IMemoryCache memoryCache) : IActiveRobotService
{
  private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

  private readonly string ActiveRobotsKey = "ActiveRobotService_ActiveRobots";
  private readonly string ActiveRobotsBriefDataKey = "ActiveRobotService_ActiveRobotsDataBrief";

    public void AddRobot(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(ActiveRobotsKey, out var activeRobotIds);
    activeRobotIds ??= [];
    activeRobotIds.Add(robotId);
    _memoryCache.Set(ActiveRobotsKey, activeRobotIds);
  }

  public void RemoveRobot(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(ActiveRobotsKey, out var activeRobotIds);
    if (activeRobotIds != null && activeRobotIds.Contains(robotId))
    {
      activeRobotIds.Remove(robotId);
      _memoryCache.Set(ActiveRobotsKey, activeRobotIds);
    }    
    _memoryCache.TryGetValue<Dictionary<Guid, RobotDataBrief>>(ActiveRobotsBriefDataKey, out var activeRobotsBriefData);
    if (activeRobotsBriefData != null && activeRobotsBriefData.ContainsKey(robotId))
    {
      activeRobotsBriefData.Remove(robotId);
      _memoryCache.Set(ActiveRobotsBriefDataKey, activeRobotsBriefData);
    }
    _memoryCache.Remove($"ActiveRobotService_RobotData_{robotId}");
  }

  public void SetRobotData(Guid robotId, RobotClientsExchange data)
  {
    _memoryCache.TryGetValue<Dictionary<Guid, RobotDataBrief>>(ActiveRobotsBriefDataKey, out var activeRobotsBriefData);
    activeRobotsBriefData ??= [];
    if (activeRobotsBriefData.TryGetValue(robotId, out RobotDataBrief? value))
    {
      value.RobotStatus = data.RobotStatus;
      value.Batteries = data.Batteries;
      value.Position = data.Position;
    }
    else 
    {
      activeRobotsBriefData[robotId] = new RobotDataBrief {
        RobotStatus = data.RobotStatus,
        Batteries = data.Batteries,
        Position = data.Position
      };
    }
    _memoryCache.Set(ActiveRobotsBriefDataKey, activeRobotsBriefData);
    _memoryCache.Set($"ActiveRobotService_RobotData_{robotId}", data);
  }

  public ReadOnlyDictionary<Guid, RobotDataBrief>? GetRobotsDataBrief()
  {
    _memoryCache.TryGetValue<Dictionary<Guid, RobotDataBrief>>(ActiveRobotsBriefDataKey, out var activeRobotsBriefData);
    return activeRobotsBriefData?.AsReadOnly();
  }

  /*
  public HashSet<Guid> GetActiveRobots()
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(ActiveRobotsKey, out var activeRobotIds);
    return activeRobotIds ?? [];
  }*/

  public bool IsRobotActive(Guid robotId)
  {
    _memoryCache.TryGetValue<HashSet<Guid>>(ActiveRobotsKey, out var activeRobotIds);
    if (activeRobotIds != null && activeRobotIds.Contains(robotId))
    {
      return true;
    }
    else 
    {
      return false;
    }
  }
}