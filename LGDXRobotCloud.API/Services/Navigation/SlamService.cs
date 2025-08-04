using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using MassTransit;

namespace LGDXRobotCloud.API.Services.Navigation;

public interface ISlamService
{
  Task UpdateMapDataAsync(Guid robotId, RobotClientsRealtimeNavResults status, RobotClientsMapData? mapData);
}

public class SlamService(
  IBus bus,
  IRobotService robotService
) : ISlamService
{
  private readonly IBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
  private readonly IRobotService _robotService = robotService ?? throw new ArgumentNullException(nameof(robotService));

  static RealtimeNavResult ConvertRealtimeNavResult(RobotClientsRealtimeNavResults realtimeNavResult)
  {
    return realtimeNavResult switch
    {
      RobotClientsRealtimeNavResults.SlamIdle => RealtimeNavResult.Idle,
      RobotClientsRealtimeNavResults.SlamRunning => RealtimeNavResult.Running,
      RobotClientsRealtimeNavResults.SlamSuccess => RealtimeNavResult.Success,
      RobotClientsRealtimeNavResults.SlamAborted => RealtimeNavResult.Aborted,
      _ => RealtimeNavResult.Idle,
    };
  }

  public async Task UpdateMapDataAsync(Guid robotId, RobotClientsRealtimeNavResults status, RobotClientsMapData? mapData)
  {
    var realmId = await _robotService.GetRobotRealmIdAsync(robotId) ?? 0;
    var navResult = ConvertRealtimeNavResult(status);
    SlamMapData? map = null;
    if (mapData != null && mapData.Data.Count > 0)
    {
      map = new SlamMapData
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
    var data = new SlamMapDataContract
    {
      RobotId = robotId,
      RealmId = realmId,
      RealtimeNavResult = navResult,
      MapData = map
    };
    await _bus.Publish(data);
  }
}