using LGDXRobotCloud.Data.Contracts;

namespace LGDXRobotCloud.UI.Services;

public interface ISlamService
{
  SlamDataContract? GetSlamData(int realmId);
  void UpdateSlamData(SlamDataContract slamData);
}

public sealed class SlamService(
    IRealTimeService realTimeService
  ) : ISlamService
{
  private readonly IRealTimeService _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));
  
  private readonly Dictionary<int, SlamDataContract> slamData = []; // RealmId, SlamMapData

  public SlamDataContract? GetSlamData(int realmId)
  {
    if (slamData.TryGetValue(realmId, out var sd))
    {
      return sd;
    }
    return null;
  }

  public void UpdateSlamData(SlamDataContract sd)
  {
    if (!slamData.ContainsKey(sd.RealmId) || sd.MapData != null)
    {
      slamData[sd.RealmId] = sd;
    }
    else
    {
      slamData[sd.RealmId].RealmId = sd.RealmId;
      slamData[sd.RealmId].RobotId = sd.RobotId;
      slamData[sd.RealmId].SlamStatus = sd.SlamStatus;
    }
    _realTimeService.SlamDataHasUpdated(new SlamDataUpdatEventArgs { RealmId = sd.RealmId });
  }
}