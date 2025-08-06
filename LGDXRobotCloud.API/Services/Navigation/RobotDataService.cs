using System.Collections.Concurrent;
using LGDXRobotCloud.Protos;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface IRobotDataService
{
  // Slam
  bool StartSlam(int realmId, Guid robotId);
  void StopSlam(int realmId);
  Guid? GetRunningSlamRobotId(int realmId);  

  IReadOnlyList<RobotClientsSlamCommands> GetSlamCommands(Guid robotId);
  void SetSlamCommands(int realmId, RobotClientsSlamCommands commands);
}

public class RobotDataService : IRobotDataService
{
  private readonly ConcurrentDictionary<int, Guid> slamRobots = []; // RealmId, RobotId
  private readonly Dictionary<Guid, ConcurrentQueue<RobotClientsSlamCommands>> slamCommands = []; // RealmId, RobotClientsSlamCommands

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
    return slamCommands[robotId].ToList().AsReadOnly();
  }

  public void SetSlamCommands(int realmId, RobotClientsSlamCommands commands)
  {
    var robotId = slamRobots[realmId];
    slamCommands[robotId].Enqueue(commands);
  }
}