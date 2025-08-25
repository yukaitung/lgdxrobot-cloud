namespace LGDXRobotCloud.Utilities.Helpers;

public static class RedisHelper
{
  public static string GetRobotDataIndex(int realmId)
  {
    return $"idxRobotData:{realmId}";
  }

  public static string GetRobotData(int realmId, Guid robotId)
  {
    return $"robotData:{realmId}:{robotId}";
  }

  public static string GetRobotDataPrefix(int realmId)
  {
    return $"robotData:{realmId}:";
  }

  public static string GetSchedulerHold(int realmId, Guid robotId)
  {
    return $"schedulerHold:{realmId}:{robotId}";
  }

  public static string GetAutoTaskUpdateQueue(int realmId)
  {
    return $"autoTaskUpdate:{realmId}";
  }

  public static string GetRobotExchangeQueue(Guid robotId)
  {
    return $"robotExchangeQueue:{robotId}";
  }

  public static string GetSlamData(int realmId)
  {
    return $"robotSlamData:{realmId}";
  }

  public static string GetSlamExchangeQueue(int realmId)
  {
    return $"robotSlamExchangeQueue:{realmId}";
  }
}