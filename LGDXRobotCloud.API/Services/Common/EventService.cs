namespace LGDXRobotCloud.API.Services.Common;

public interface IEventService
{
  event EventHandler AutoTaskCreated;
  void AutoTaskHasCreated();

  event EventHandler<Guid> RobotCommandsUpdated;
  void RobotCommandsHasUpdated(Guid robotId);

  event EventHandler<Guid> RobotHasNextTask;
  void RobotHasNextTaskTriggered(Guid robotId);

  event EventHandler<Guid> SlamCommandsUpdated;
  void SlamCommandsHasUpdated(Guid robotId);
}

public class EventService : IEventService
{
  public event EventHandler? AutoTaskCreated;
  public void AutoTaskHasCreated()
  {
    AutoTaskCreated?.Invoke(this, EventArgs.Empty);
  }

  public event EventHandler<Guid>? RobotCommandsUpdated;
  public void RobotCommandsHasUpdated(Guid robotId)
  {
    RobotCommandsUpdated?.Invoke(this, robotId);
  }

  public event EventHandler<Guid>? RobotHasNextTask;
  public void RobotHasNextTaskTriggered(Guid robotId)
  {
    RobotHasNextTask?.Invoke(this, robotId);
  }

  public event EventHandler<Guid>? SlamCommandsUpdated;
  public void SlamCommandsHasUpdated(Guid robotId)
  {
    SlamCommandsUpdated?.Invoke(this, robotId);
  }
}