using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using MassTransit;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface ISlamService
{
  Task<bool> StartSlamAsync(Guid robotId);
  Task StopSlamAsync(Guid robotId);

  // Client to Server
  Task UpdateSlamDataAsync(Guid robotId, RobotClientsSlamStatus status, RobotClientsMapData mapData);

  // Server to Client
  IReadOnlyList<RobotClientsSlamCommands> GetSlamCommands(Guid robotId);
  bool SetSlamCommands(int realmId, RobotClientsSlamCommands commands);
}

public class SlamService(
  IBus bus,
  IEventService eventService,
  IRobotDataService robotDataService,
  IRobotService robotService
) : ISlamService
{
  private readonly IBus _bus = bus;
  private readonly IEventService _eventService = eventService;
  private readonly IRobotDataService _robotDataService = robotDataService;
  private readonly IRobotService _robotService = robotService;

  static SlamStatus ConvertSlamStatus(RobotClientsSlamStatus slamStatus)
  {
    return slamStatus switch
    {
      RobotClientsSlamStatus.SlamIdle => SlamStatus.Idle,
      RobotClientsSlamStatus.SlamRunning => SlamStatus.Running,
      RobotClientsSlamStatus.SlamSuccess => SlamStatus.Success,
      RobotClientsSlamStatus.SlamAborted => SlamStatus.Aborted,
      _ => SlamStatus.Idle,
    };
  }

  public async Task<bool> StartSlamAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    return _robotDataService.StartSlam(realmId, robotId);
  }

  public async Task StopSlamAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    _robotDataService.StopSlam(realmId);
  }

  public async Task UpdateSlamDataAsync(Guid robotId, RobotClientsSlamStatus status, RobotClientsMapData mapData)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var slamStatus = ConvertSlamStatus(status);
    MapData? map = null;
    if (mapData.Data.Count > 0)
    {
      map = new MapData
      {
        Resolution = mapData.Resolution,
        Width = mapData.Width,
        Height = mapData.Height,
        Origin = new RobotDof
        {
          X = mapData.Origin.X,
          Y = mapData.Origin.Y,
          Rotation = mapData.Origin.Rotation,
        },
        Data = [.. mapData.Data.Select(x => (short)x)]
      };
    }
    var data = new SlamDataContract
    {
      RobotId = robotId,
      RealmId = realmId,
      SlamStatus = slamStatus,
      MapData = map
    };
    await _bus.Publish(data);
  }

  public IReadOnlyList<RobotClientsSlamCommands> GetSlamCommands(Guid robotId)
  {
    return _robotDataService.GetSlamCommands(robotId);
  }

  public bool SetSlamCommands(int realmId, RobotClientsSlamCommands commands)
  {
    var robotId = _robotDataService.GetRunningSlamRobotId(realmId);
    if (robotId == null)
    {
      return false;
    }
    _robotDataService.SetSlamCommands(realmId, commands);
    _eventService.SlamCommandsHasUpdated(robotId.Value);
    return true;
  }
}