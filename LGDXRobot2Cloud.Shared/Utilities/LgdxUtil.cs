using LGDXRobot2Cloud.Shared.Enums;

namespace LGDXRobot2Cloud.Shared.Utilities;

public class LgdxUtil
{
  public readonly static List<int> AutoTaskRunningStateList = [(int)ProgressState.Template, (int)ProgressState.Waiting, (int)ProgressState.Completed, (int)ProgressState.Aborted];
}
