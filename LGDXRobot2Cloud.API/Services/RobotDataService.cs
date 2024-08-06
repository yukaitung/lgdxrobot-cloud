using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using LGDXRobot2Cloud.Shared.Entities;

namespace LGDXRobot2Cloud.API.Services
{
  public interface IRobotDataService
  {
    void SetRobotData(Guid robotId, RobotData position);
    RobotData? GetRobotData(Guid robotId);
    ReadOnlyDictionary<Guid, RobotData> GetRobotsData();
  }
  
  public class RobotDataService : IRobotDataService
  {
    private readonly ConcurrentDictionary<Guid, RobotData> _robotsData;

    public RobotDataService()
    {
      _robotsData = new ConcurrentDictionary<Guid, RobotData>();
    }
    
    public void SetRobotData(Guid robotId, RobotData position)
    {
      _robotsData.AddOrUpdate(robotId, position, (key, oldValue) => position);
    }

    public RobotData? GetRobotData(Guid robotId)
    {
      if(_robotsData.TryGetValue(robotId, out RobotData? position))
        return position;
      else
        return null;
    }

    public ReadOnlyDictionary<Guid, RobotData> GetRobotsData()
    {
      return _robotsData.AsReadOnly();
    }
  }
}