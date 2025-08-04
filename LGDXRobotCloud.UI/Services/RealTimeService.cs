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

public class SlamMapDataUpdatEventArgs : EventArgs
{
  public required int RealmId { get; set; }
}

public interface IRealTimeService
{
  event EventHandler<RobotUpdatEventArgs> RobotDataUpdated;
  event EventHandler<RobotUpdatEventArgs> RobotCommandsUpdated;
  event EventHandler<AutoTaskUpdatEventArgs> AutoTaskUpdated;
  event EventHandler<SlamMapDataUpdatEventArgs> SlamMapDataUpdated;

  void RobotDataHasUpdated(RobotUpdatEventArgs robotUpdatEventArgs);
  void RobotCommandsHasUpdated(RobotUpdatEventArgs robotUpdatEventArgs);
  void AutoTaskHasUpdated(AutoTaskUpdatEventArgs autoTaskUpdatEventArgs);
  void SlamMapDataHasUpdated(SlamMapDataUpdatEventArgs slamMapDataUpdatEventArgs);
}

public sealed class RealTimeService : IRealTimeService
{
  public event EventHandler<RobotUpdatEventArgs>? RobotDataUpdated;
  public event EventHandler<RobotUpdatEventArgs>? RobotCommandsUpdated;
  public event EventHandler<AutoTaskUpdatEventArgs>? AutoTaskUpdated;
  public event EventHandler<SlamMapDataUpdatEventArgs>? SlamMapDataUpdated;

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

  public void SlamMapDataHasUpdated(SlamMapDataUpdatEventArgs slamMapDataUpdatEventArgs)
  {
    SlamMapDataUpdated?.Invoke(this, slamMapDataUpdatEventArgs);
  }
}