namespace LGDXRobotCloud.UI.Helpers;

public static class UiHelper 
{
  public static string TimeToString(DateTimeOffset? time)
  {
    if (time == null)
      return "-";
    return time.Value.ToString("yyyy-MM-dd HH:mm:ss");
  }
}