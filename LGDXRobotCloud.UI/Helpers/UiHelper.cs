using System.Globalization;

namespace LGDXRobotCloud.UI.Helpers;

public static class UiHelper 
{
  public static string TimeToString(TimeZoneInfo timeZoneInfo, DateTimeOffset? time)
  {
    if (time == null)
      return "-";
    time = time.Value.ToOffset(timeZoneInfo.GetUtcOffset(time.Value));
    return time.Value.DateTime.ToString(CultureInfo.CurrentCulture);
  }
}