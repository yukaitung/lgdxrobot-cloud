using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Wolverine;

namespace LGDXRobotCloud.API.UnitTests.Services.Automation;

public class AutoTaskSchedulerServiceTests
{
  private static readonly Guid RobotHasNoTaskGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
  private static readonly Guid RobotHasRunningTaskGuid = Guid.Parse("6ebff82e-0c12-4d35-9b4c-69452bc1d4e4");
  private static readonly Guid RobotHasRunningLastStepTaskGuid = Guid.Parse("6ac8afe8-6532-4c04-b176-1c2cd0dc484e");
  private static readonly Guid RobotHasTriggerTaskGuid = Guid.Parse("019558f3-cf79-7783-9730-d6f44aea744a");
  private static readonly Guid RobotHasRunningTriggerTaskGuid = Guid.Parse("019558dc-db66-7aef-935b-e8f9a743081d");
  private static readonly Guid RobotHasRunningApiControlTaskGuid = Guid.Parse("019558f6-cfb2-7a59-aeac-498ee98781fb");
  private static readonly Guid RobotHasRunningTaskPreMovingGuid = Guid.Parse("019558fc-9b73-7bb2-b0e0-d5282571aa4c");

  private readonly List<AutoTask> autoTasks = [
    new() 
    {
      Id = 1,
      Name = "Waiting Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Waiting,
    },
    new() 
    {
      Id = 2,
      Name = "Running Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Moving,
      CurrentProgressOrder = 0,
      NextToken = "Next Token 1",
      AssignedRobotId = RobotHasRunningTaskGuid
    },
    new() 
    {
      Id = 3,
      Name = "Running Task Last Step",
      Priority = 0,
      FlowId = 1,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Completing,
      CurrentProgressOrder = 1,
      NextToken = "Next Token 2",
      AssignedRobotId = RobotHasRunningLastStepTaskGuid
    },
    new() 
    {
      Id = 4,
      Name = "Waiting Task Trigger",
      Priority = 0,
      FlowId = 2,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Waiting,
      AssignedRobotId = RobotHasTriggerTaskGuid
    },
    new() 
    {
      Id = 5,
      Name = "Running Task Trigger",
      Priority = 0,
      FlowId = 2,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Moving,
      CurrentProgressOrder = 0,
      NextToken = "Next Token 5",
      AssignedRobotId = RobotHasRunningTriggerTaskGuid
    },
    new() 
    {
      Id = 6,
      Name = "Running Task API",
      Priority = 0,
      FlowId = 3,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.Moving,
      CurrentProgressOrder = 0,
      NextToken = "Next Token 6",
      AssignedRobotId = RobotHasRunningApiControlTaskGuid
    },
    new() 
    {
      Id = 7,
      Name = "Running Task PreMoving",
      Priority = 0,
      FlowId = 4,
      RealmId = 0,
      CurrentProgressId = (int)ProgressState.PreMoving,
      CurrentProgressOrder = 0,
      NextToken = "Next Token 6",
      AssignedRobotId = RobotHasRunningTaskPreMovingGuid
    },
  ];

  private readonly List<AutoTaskDetail> autoTaskDetails = [
    new()
    {
      Id = 1,
      Order = 0,
      CustomX = 0,
      CustomY = 0,
      CustomRotation = 0,
      AutoTaskId = 1,
    },
    new()
    {
      Id = 2,
      Order = 1,
      CustomX = 1,
      AutoTaskId = 1,
      WaypointId = 1,
    },
    new()
    {
      Id = 3,
      Order = 2,
      CustomY = 1,
      AutoTaskId = 1,
      WaypointId = 2,
    },
    new()
    {
      Id = 4,
      Order = 3,
      CustomRotation = 1,
      AutoTaskId = 1,
      WaypointId = 3,
    },
    new()
    {
      Id = 5,
      Order = 4,
      CustomY = 0,
      CustomRotation = 0,
      AutoTaskId = 1,
    },
    new()
    {
      Id = 6,
      Order = 5,
      CustomX = 0,
      CustomRotation = 0,
      AutoTaskId = 1,
    },
    new()
    {
      Id = 7,
      Order = 6,
      CustomX = 0,
      CustomY = 0,
      AutoTaskId = 1,
    },
    new()
    {
      Id = 8,
      Order = 0,
      AutoTaskId = 7,
      WaypointId = 1,
    },
    new()
    {
      Id = 9,
      Order = 1,
      AutoTaskId = 7,
      WaypointId = 2,
    },
  ];

  private readonly List<Progress> progresses = [
    new ()
    {
      Id = (int)ProgressState.Completed,
      Name = "Completed",
      System = true,
      Reserved = true
    },
    new ()
    {
      Id = (int)ProgressState.Aborted,
      Name = "Aborted",
      System = true,
      Reserved = true
    },
    new ()
    {
      Id = (int)ProgressState.PreMoving,
      Name = "PreMoving",
      System = true
    },
    new ()
    {
      Id = (int)ProgressState.Moving,
      Name = "Moving",
      System = true
    },
    new ()
    {
      Id = (int)ProgressState.Completing,
      Name = "Completing",
      System = true
    },
  ];

  private readonly List<Flow> flows = [
    new ()
    {
      Id = 1,
      Name = "Flow 1"
    },
    new ()
    {
      Id = 2,
      Name = "Flow 2 Has Trigger"
    },
    new ()
    {
      Id = 3,
      Name = "Flow 3 API Control"
    },
    new ()
    {
      Id = 4,
      Name = "Flow 4 PreMoving"
    }
  ];

  private readonly List<FlowDetail> flowDetails = [
    new()
    {
      Id = 1,
      Order = 0,
      ProgressId = (int)ProgressState.Moving,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 1
    },
    new()
    {
      Id = 2,
      Order = 1,
      ProgressId = (int)ProgressState.Completing,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 1
    },
    new()
    {
      Id = 3,
      Order = 0,
      ProgressId = (int)ProgressState.Moving,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 2,
      TriggerId = 1
    },
    new()
    {
      Id = 4,
      Order = 0,
      ProgressId = (int)ProgressState.Moving,
      AutoTaskNextControllerId = (int)AutoTaskNextController.API,
      FlowId = 3
    },
    new()
    {
      Id = 5,
      Order = 0,
      ProgressId = (int)ProgressState.PreMoving,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 4
    }
  ];

  private readonly List<Robot> robots = [
    new() {
      Id = RobotHasNoTaskGuid,
      Name = "Test Robot 1",
    },
    new() {
      Id = RobotHasRunningTaskGuid,
      Name = "Test Robot 2",
    },
    new() {
      Id = RobotHasRunningLastStepTaskGuid,
      Name = "Test Robot 3",
    },
    new() {
      Id = RobotHasTriggerTaskGuid,
      Name = "Test Robot 4",
    },
    new() {
      Id = RobotHasRunningTriggerTaskGuid,
      Name = "Test Robot 5",
    },
    new() {
      Id = RobotHasRunningApiControlTaskGuid,
      Name = "Test Robot 6",
    },
    new() {
      Id = RobotHasRunningTaskPreMovingGuid,
      Name = "Test Robot 7",
    }
  ];

  private readonly List<Waypoint> waypoints = [
    new()
    {
      Id = 1,
      Name = "1",
      X = 43,
      Y = 56,
      Rotation = 0.233,
    },
    new()
    {
      Id = 2,
      Name = "2",
      X = 54,
      Y = 453,
      Rotation = 0.786,
    },
    new()
    {
      Id = 3,
      Name = "3",
      X = 5,
      Y = 9,
      Rotation = 0,
    },
    new()
    {
      Id = 4,
      Name = "4",
      X = 7,
      Y = 98,
      Rotation = 0.676,
    },
    new()
    {
      Id = 5,
      Name = "5",
      X = 0,
      Y = 0,
      Rotation = 0,
    },
  ];

  private List<RobotClientsDof> robotClientsDofs = [];

  private readonly Mock<IMessageBus> mockBus = new();
  private readonly Mock<IEmailService> mockEmailService = new();
  private readonly Mock<IMemoryCache> mockMemoryCache = new();
  private readonly Mock<IOnlineRobotsService> mockOnlineRobotService = new();
  private readonly Mock<IRobotService> mockRobotService = new();
  private readonly Mock<ITriggerService> mockTriggerService = new();
  private readonly Mock<IAutoTaskPathPlannerService> mockAutoTaskPathPlanner = new();
  private readonly LgdxContext lgdxContext;

  public AutoTaskSchedulerServiceTests()
  {
    mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.Set<AutoTaskDetail>().AddRange(autoTaskDetails);
    lgdxContext.Set<Progress>().AddRange(progresses);
    lgdxContext.Set<Flow>().AddRange(flows);
    lgdxContext.Set<FlowDetail>().AddRange(flowDetails);
    lgdxContext.Set<Robot>().AddRange(robots);
    lgdxContext.Set<Waypoint>().AddRange(waypoints);
    lgdxContext.SaveChanges();
  }

  private List<RobotClientsPath> GeneratePath(AutoTask autoTask)
  {
    var id = autoTask.Id;
    var details = autoTask.AutoTaskDetails
      .Where(atd => atd.AutoTaskId == id)
      .Select(atd => atd.WaypointId)
      .ToList();

    List<RobotClientsPath> result = [];
    foreach (var waypointId in details)
    {
      var waypoint = waypoints.FirstOrDefault(w => w.Id == waypointId);
      result.Add(new RobotClientsPath
      {
        Waypoints = { new RobotClientsDof
        {
          X = 0,
          Y = 0,
          Rotation = 0
        }}
      });
    }
    return result;
  }

}