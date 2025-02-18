using LGDXRobotCloud.Data.Contracts;

namespace LGDXRobotCloud.UI.Services;

public class RobotUpdatEventArgs : EventArgs
{
  public required Guid RobotId { get; set; }
  public required int RealmId { get; set; }
}

public class AutoTaskUpdatEventArgs : EventArgs
{
  public required AutoTaskUpdateContract AutoTaskUpdateContract { get; set; }
}

public interface IRealTimeService
{
  event EventHandler<RobotUpdatEventArgs> RobotDataUpdated;
  event EventHandler<RobotUpdatEventArgs> RobotCommandsUpdated;
  event EventHandler<AutoTaskUpdatEventArgs> AutoTaskUpdated;
  void RobotDataHasUpdated(RobotUpdatEventArgs robotUpdatEventArgs);
  void RobotCommandsHasUpdated(RobotUpdatEventArgs robotUpdatEventArgs);
  void AutoTaskHasUpdated(AutoTaskUpdatEventArgs autoTaskUpdatEventArgs);
}

public sealed class RealTimeService : IRealTimeService
{
  public event EventHandler<RobotUpdatEventArgs>? RobotDataUpdated;
  public event EventHandler<RobotUpdatEventArgs>? RobotCommandsUpdated;
  public event EventHandler<AutoTaskUpdatEventArgs>? AutoTaskUpdated;

  public void RobotDataHasUpdated(RobotUpdatEventArgs robotUpdatEventArgs)
  {
    RobotDataUpdated?.Invoke(this, robotUpdatEventArgs);
  }

  public void RobotCommandsHasUpdated(RobotUpdatEventArgs robotUpdatEventArgs)
  {
    RobotCommandsUpdated?.Invoke(this, robotUpdatEventArgs);
  }

  public void AutoTaskHasUpdated(AutoTaskUpdatEventArgs autoTaskUpdatEventArgs)
  {
    AutoTaskUpdated?.Invoke(this, autoTaskUpdatEventArgs);
  }
}