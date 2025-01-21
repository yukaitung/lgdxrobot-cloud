namespace LGDXRobot2Cloud.UI.Services;

public class RobotUpdatEventArgs : EventArgs
{
  public required Guid RobotId { get; set; }
  public required int RealmId { get; set; }
}

public interface IRealTimeService
{
  event EventHandler<RobotUpdatEventArgs> RobotDataUpdated;
  event EventHandler<RobotUpdatEventArgs> RobotCommandsUpdated;
  void RobotDataHasUpdated(RobotUpdatEventArgs robotId);
  void RobotCommandsHasUpdated(RobotUpdatEventArgs robotId);
}

public sealed class RealTimeService : IRealTimeService
{
  public event EventHandler<RobotUpdatEventArgs>? RobotDataUpdated;
  public event EventHandler<RobotUpdatEventArgs>? RobotCommandsUpdated;

  public void RobotDataHasUpdated(RobotUpdatEventArgs robotId)
  {
    RobotDataUpdated?.Invoke(this, robotId);
  }

  public void RobotCommandsHasUpdated(RobotUpdatEventArgs robotId)
  {
    RobotCommandsUpdated?.Invoke(this, robotId);
  }
}