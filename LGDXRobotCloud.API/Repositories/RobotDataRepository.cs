using System.Collections.Concurrent;
using LGDXRobotCloud.Protos;

namespace LGDXRobotCloud.API.Repositories;

public interface IRobotDataRepository
{
  // Exchange
  void StartExchange(int realmId, Guid robotId);
  void StopExchange(int realmId, Guid robotId);

  IReadOnlyList<Guid> GetOnlineRobots(int realmId);
  RobotClientsData? GetRobotData(Guid robotId);
  bool SetRobotData(Guid robotId, RobotClientsData data);

  bool AutoTaskSchedulerHoldRobot(int realmId, Guid robotId);
  void AutoTaskScheduleReleaseRobot(int realmId, Guid robotId);

  IReadOnlyList<RobotClientsRobotCommands> GetRobotCommands(Guid robotId);
  bool SetRobotCommands(Guid robotId, RobotClientsRobotCommands cmd);
  IReadOnlyList<RobotClientsAutoTask> GetAutoTasks(Guid robotId);
  bool SetAutoTasks(Guid robotId, RobotClientsAutoTask autoTask);

  // Slam
  bool StartSlam(int realmId, Guid robotId);
  void StopSlam(int realmId);
  Guid? GetRunningSlamRobotId(int realmId);

  IReadOnlyList<RobotClientsSlamCommands> GetSlamCommands(Guid robotId);
  void SetSlamCommands(int realmId, RobotClientsSlamCommands commands);
}

public class RobotDataRepository : IRobotDataRepository
{
  private readonly Dictionary<Guid, ConcurrentQueue<RobotClientsRobotCommands>> commands = []; // RobotId, RobotClientsRobotCommands
  private readonly Dictionary<Guid, ConcurrentQueue<RobotClientsAutoTask>> autoTasks = []; // RobotId, RobotClientsAutoTask

  private readonly ConcurrentDictionary<int, HashSet<Guid>> onlineRobots = []; // RealmId, OnlineRobotsIds
  private readonly ConcurrentDictionary<int, HashSet<Guid>> autoTaskSchedulerHold = []; // RealmId, OnlineRobotsIds
  private readonly Dictionary<Guid, RobotClientsData> robotData = []; // RobotId, RobotClientsData

  private readonly ConcurrentDictionary<int, Guid> slamRobots = []; // RealmId, RobotId
  private readonly Dictionary<Guid, ConcurrentQueue<RobotClientsSlamCommands>> slamCommands = []; // RobotId, RobotClientsSlamCommands

  public void StartExchange(int realmId, Guid robotId)
  {
    if (onlineRobots.TryGetValue(realmId, out HashSet<Guid>? OnlineRobotsIds))
    {
      OnlineRobotsIds.Add(robotId);
    }
    else
    {
      onlineRobots[realmId] = [robotId];
    }

    commands.Add(robotId, new ConcurrentQueue<RobotClientsRobotCommands>());
    autoTasks.Add(robotId, new ConcurrentQueue<RobotClientsAutoTask>());
    robotData.Add(robotId, new RobotClientsData());
  }

  public void StopExchange(int realmId, Guid robotId)
  {
    if (onlineRobots.TryGetValue(realmId, out HashSet<Guid>? OnlineRobotsIds))
    {
      OnlineRobotsIds.Remove(robotId);
    }

    commands.Remove(robotId, out _);
    autoTasks.Remove(robotId, out _);
    robotData.Remove(robotId, out _);
  }

  public IReadOnlyList<Guid> GetOnlineRobots(int realmId)
  {
    if (onlineRobots.TryGetValue(realmId, out HashSet<Guid>? OnlineRobotsIds))
    {
      return [.. OnlineRobotsIds];
    }
    else
    {
      return [];
    }
  }

  public RobotClientsData? GetRobotData(Guid robotId)
  {
    if (robotData.TryGetValue(robotId, out RobotClientsData? data))
    {
      return data;
    }
    else
    {
      return null;
    }
  }

  public bool SetRobotData(Guid robotId, RobotClientsData data)
  {
    if (robotData.TryGetValue(robotId, out RobotClientsData? _))
    {
      robotData[robotId] = data;
      return true;
    }
    else
    {
      return false;
    }
  }

  public bool AutoTaskSchedulerHoldRobot(int realmId, Guid robotId)
  {
    if (autoTaskSchedulerHold.TryGetValue(realmId, out HashSet<Guid>? OnlineRobotsIds))
    {
      if (OnlineRobotsIds.Contains(robotId))
      {
        // Cannot hold the robot
        return false;
      }
      OnlineRobotsIds.Add(robotId);
      return true;
    }
    else
    {
      autoTaskSchedulerHold[realmId] = [robotId];
      return true;
    }
  }

  public void AutoTaskScheduleReleaseRobot(int realmId, Guid robotId)
  {
    // Release the robot
    if (autoTaskSchedulerHold.TryGetValue(realmId, out HashSet<Guid>? OnlineRobotsIds))
    {
      OnlineRobotsIds.Remove(robotId);
    }
  }

  public IReadOnlyList<RobotClientsRobotCommands> GetRobotCommands(Guid robotId)
  {
    List<RobotClientsRobotCommands> result = [];
    int count = commands.Count;
    for (int i = 0; i < count; i++)
    {
      if (commands[robotId].TryDequeue(out var cmd))
      {
        result.Add(cmd);
      }
    }
    return result.AsReadOnly();
  }

  public bool SetRobotCommands(Guid robotId, RobotClientsRobotCommands cmd)
  {
    if (commands.TryGetValue(robotId, out ConcurrentQueue<RobotClientsRobotCommands>? value))
    {
      value.Enqueue(cmd);
      return true;
    }
    else
    {
      return false;
    }
  }

  public IReadOnlyList<RobotClientsAutoTask> GetAutoTasks(Guid robotId)
  {
    List<RobotClientsAutoTask> result = [];
    if (autoTasks.TryGetValue(robotId, out var autoTasksForRobot))
    {
      int count = autoTasksForRobot.Count;
      for (int i = 0; i < count; i++)
      {
        if (autoTasksForRobot.TryDequeue(out var autoTask))
        {
          result.Add(autoTask);
        }
      }
    }
    return result.AsReadOnly();
  }

  public bool SetAutoTasks(Guid robotId, RobotClientsAutoTask autoTask)
  {
    if (autoTasks.TryGetValue(robotId, out ConcurrentQueue<RobotClientsAutoTask>? value))
    {
      value.Enqueue(autoTask);
      return true;
    }
    else
    {
      return false;
    }
  }

  public bool StartSlam(int realmId, Guid robotId)
  {
    // Only one robot can running SLAM at a time
    if (slamRobots.ContainsKey(realmId))
    {
      return false;
    }
    slamRobots[realmId] = robotId;
    slamCommands.Add(robotId, new ConcurrentQueue<RobotClientsSlamCommands>());
    return true;
  }

  public void StopSlam(int realmId)
  {
    var robotId = slamRobots[realmId];
    slamRobots.Remove(realmId, out _);
    slamCommands.Remove(robotId, out _);
  }

  public Guid? GetRunningSlamRobotId(int realmId)
  {
    if (slamRobots.TryGetValue(realmId, out var robotId))
    {
      return robotId;
    }
    return null;
  }

  public IReadOnlyList<RobotClientsSlamCommands> GetSlamCommands(Guid robotId)
  {
    List<RobotClientsSlamCommands> result = [];
    int count = slamCommands.Count;
    for (int i = 0; i < count; i++)
    {
      if (slamCommands[robotId].TryDequeue(out var commands))
      {
        result.Add(commands);
      }
    }
    return result.AsReadOnly();
  }

  public void SetSlamCommands(int realmId, RobotClientsSlamCommands commands)
  {
    var robotId = slamRobots[realmId];
    slamCommands[robotId].Enqueue(commands);
  }
}