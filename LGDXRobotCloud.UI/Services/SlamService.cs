using LGDXRobotCloud.Data.Contracts;

namespace LGDXRobotCloud.UI.Services;

public interface ISlamService
{
  SlamMapDataContract? GetSlamData(int realmId);
  void UpdateSlamData(SlamMapDataContract slamMapData);
}

public sealed class SlamService(
    IRealTimeService realTimeService
  ) : ISlamService
{
  private readonly IRealTimeService _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));
  
  private readonly Dictionary<int, SlamMapDataContract> slamData = []; // RealmId, SlamMapData

  public SlamMapDataContract? GetSlamData(int realmId)
  {
    if (slamData.TryGetValue(realmId, out var sd))
    {
      return sd;
    }
    return null;
  }

  public void UpdateSlamData(SlamMapDataContract slamMapData)
  {
    if (!slamData.ContainsKey(slamMapData.RealmId) || slamMapData.MapData != null)
    {
      slamData[slamMapData.RealmId] = slamMapData;
    }
    else
    {
      slamData[slamMapData.RealmId].RealmId = slamMapData.RealmId;
      slamData[slamMapData.RealmId].RobotId = slamMapData.RobotId;
      slamData[slamMapData.RealmId].RealtimeNavResult = slamMapData.RealtimeNavResult;
    }
    _realTimeService.SlamMapDataHasUpdated(new SlamMapDataUpdatEventArgs { RealmId = slamMapData.RealmId });
  }
}