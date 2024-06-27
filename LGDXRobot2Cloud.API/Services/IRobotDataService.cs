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
}