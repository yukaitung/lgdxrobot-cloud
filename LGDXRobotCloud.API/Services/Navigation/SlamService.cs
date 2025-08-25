using LGDXRobotCloud.API.Repositories;
using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface ISlamService
{
  Task<bool> StartSlamAsync(Guid robotId);
  Task StopSlamAsync(Guid robotId);

  // Client to Server
  Task UpdateSlamDataAsync(Guid robotId, RobotClientsSlamStatus status, RobotClientsMapData? mapData);

  // Server to Client
  Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands);
}

public class SlamService(
  ISlamDataRepository slamDataRepository,
  IRobotService robotService
) : ISlamService
{
  private readonly ISlamDataRepository _slamDataRepository = slamDataRepository;
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
    return await _slamDataRepository.StartSlamAsync(realmId, robotId);
  }

  public async Task StopSlamAsync(Guid robotId)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    await _slamDataRepository.StopSlamAsync(realmId, robotId);
  }

  public async Task UpdateSlamDataAsync(Guid robotId, RobotClientsSlamStatus status, RobotClientsMapData? mapData)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var slamStatus = ConvertSlamStatus(status);
    MapData? map = null;
    if (mapData != null && mapData.Data.Count > 0)
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
    var data = new SlamData
    {
      RobotId = robotId,
      RealmId = realmId,
      SlamStatus = slamStatus,
      MapData = map
    };
    await _slamDataRepository.SetSlamExchangeAsync(realmId, data);
  }

  public async Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands)
  {
    return await _slamDataRepository.AddSlamCommandAsync(realmId, commands);
  }
}