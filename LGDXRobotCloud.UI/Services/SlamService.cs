using LGDXRobotCloud.Data.Contracts;

namespace LGDXRobotCloud.UI.Services;

public interface ISlamService
{
  void StartSlam(int realmId);
  SlamDataContract? GetSlamData(int realmId);
  void UpdateSlamData(SlamDataContract slamData);
  void StopSlam(int realmId);
}

public sealed class SlamService(
    IRealTimeService realTimeService
  ) : ISlamService
{
  private readonly IRealTimeService _realTimeService = realTimeService ?? throw new ArgumentNullException(nameof(realTimeService));

  private readonly Dictionary<int, SlamDataContract> slamData = []; // RealmId, SlamMapData

  public void StartSlam(int realmId)
  {
    if (!slamData.ContainsKey(realmId))
    {
      slamData[realmId] = new SlamDataContract();
    }
  }

  public void StopSlam(int realmId)
  {
    slamData.Remove(realmId);
    // Redirect the user to the Realm page
  }

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
    if (!slamData.ContainsKey(sd.RealmId))
    {
      // Ignore the update
      return;
    }
    
    if (sd.MapData != null)
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