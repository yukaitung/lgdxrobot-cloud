using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Utilities.Helpers;

public static class LgdxHelper
{
  public readonly static List<int> AutoTaskRunningStateList = [(int)ProgressState.Template, (int)ProgressState.Waiting, (int)ProgressState.Completed, (int)ProgressState.Aborted];
}
