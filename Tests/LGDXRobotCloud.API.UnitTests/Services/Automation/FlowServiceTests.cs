using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Automation;
using LGDXRobotCloud.Utilities.Enums;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Automation;

public class FlowServiceTests
{
  private readonly List<Flow> flows = [
    new()
    {
      Id = 1,
      Name = "Flow 1"
    },
    new()
    {
      Id = 2,
      Name = "Flow 2"
    }
  ];

  private readonly List<FlowDetail> flowDetails = [
    new()
    {
      Id = 1,
      Order = 0,
      ProgressId = (int)ProgressState.Starting,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      TriggerId = 1,
      FlowId = 1
    },
    new()
    {
      Id = 2,
      Order = 1,
      ProgressId = (int)ProgressState.Moving,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 1
    },
    new()
    {
      Id = 3,
      Order = 2,
      ProgressId = (int)ProgressState.Completing,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 1
    },
    new()
    {
      Id = 4,
      Order = 0,
      ProgressId = (int)ProgressState.Moving,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 2
    },
  ];

  private readonly List<Progress> progresses = [
    new ()
    {
      Id = (int)ProgressState.Template,
      Name = "Template",
      System = true,
      Reserved = true
    },
    new ()
    {
      Id = (int)ProgressState.Starting,
      Name = "Starting",
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
    }
  ];

  private readonly List<Trigger> triggers = [
    new()
    {
      Id = 1,
      Name = "Trigger",
      Url = "https://www.example.com",
      HttpMethodId = (int)TriggerHttpMethod.Get
    }
  ];

  private readonly List<AutoTask> autoTasks = [
    new() 
    {
      Id = 1,
      Name = "Completed Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      CurrentProgressId = (int)ProgressState.Completed,
    },
    new() 
    {
      Id = 2,
      Name = "Waiting Task",
      Priority = 0,
      FlowId = 2,
      RealmId = 1,
      CurrentProgressId = (int)ProgressState.Waiting,
    }
  ];

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly LgdxContext lgdxContext;

  public FlowServiceTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<Flow>().AddRange(flows);
    lgdxContext.Set<FlowDetail>().AddRange(flowDetails);
    lgdxContext.Set<Progress>().AddRange(progresses);
    lgdxContext.Set<Trigger>().AddRange(triggers);
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData("")]
  [InlineData("Flow")]
  [InlineData("Flow 1")]
  [InlineData("321")]
  public async Task GetFlowsAsync_ShouldReturnsFlows(string flowName)
  {
    // Arrange
    var expected = flows.Where(t => t.Name!.Contains(flowName));
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    var (actual, _) = await flowService.GetFlowsAsync(flowName, 1, 10);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
    });
  }

  [Theory]
  [InlineData(1)]
  public async Task GetAutoTaskAsync_CalledWithValidId_ShouldReturnsAutoTask(int flowId)
  {
    // Arrange
    var expected = flows.FirstOrDefault(t => t.Id == flowId);
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await flowService.GetFlowAsync(flowId);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Id, actual.Id);
    Assert.All(actual.FlowDetails, a => {
      var e = expected!.FlowDetails.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Order, a.Order);
      Assert.Equal(e.ProgressId, a.ProgressId);
      Assert.Equal(e.AutoTaskNextControllerId, a.AutoTaskNextControllerId);
      Assert.Equal(e.TriggerId, a.TriggerId);
    });
  }

  [Fact]
  public async Task GetAutoTaskAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => flowService.GetFlowAsync(flows.Count + 1);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task CreateFlowAsync_CalledWithValidFlow_ShouldReturnsFlow()
  {
    // Arrange
    var expected = new FlowCreateBusinessModel {
      Name = "Test Flow",
      FlowDetails = [new FlowDetailCreateBusinessModel {
        Order = 0,
        ProgressId = (int)ProgressState.Moving,
        AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
        TriggerId = 1
      }]
    };
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await flowService.CreateFlowAsync(expected);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected.Name, actual.Name);
    Assert.Single(actual.FlowDetails);
    Assert.All(actual.FlowDetails, a => {
      var e = expected.FlowDetails.FirstOrDefault();
      Assert.NotNull(e);
      Assert.Equal(e.Order, a.Order);
      Assert.Equal(e.ProgressId, a.ProgressId);
      Assert.Equal(e.AutoTaskNextControllerId, a.AutoTaskNextControllerId);
      Assert.Equal(e.TriggerId, a.TriggerId);
    });
  }

  [Fact]
  public async Task CreateFlowAsync_CalledWithReservedProgress_ShouldThrowsValidationExpection()
  {
    // Arrange
    int progressId = (int)ProgressState.Template;
    var expected = new FlowCreateBusinessModel {
      Name = "Test Flow",
      FlowDetails = [new FlowDetailCreateBusinessModel {
        Order = 0,
        ProgressId = progressId,
        AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
        TriggerId = 1
      }]
    };
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => flowService.CreateFlowAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"The Progress ID: {progressId} is reserved.", exception.Message);
  }

  [Fact]
  public async Task CreateFlowAsync_CalledWithInvalidProgress_ShouldThrowsValidationExpection()
  {
    // Arrange
    int progressId = 0;
    var expected = new FlowCreateBusinessModel {
      Name = "Test Flow",
      FlowDetails = [new FlowDetailCreateBusinessModel {
        Order = 0,
        ProgressId = progressId,
        AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
        TriggerId = 1
      }]
    };
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => flowService.CreateFlowAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"The Progress Id: {progressId} is invalid.", exception.Message);
  }

  [Fact]
  public async Task CreateFlowAsync_CalledWithInvalidTrigger_ShouldThrowsValidationExpection()
  {
    // Arrange
    int triggerId = triggers.Count + 1;
    var expected = new FlowCreateBusinessModel {
      Name = "Test Flow",
      FlowDetails = [new FlowDetailCreateBusinessModel {
        Order = 0,
        ProgressId = (int)ProgressState.Moving,
        AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
        TriggerId = triggerId
      }]
    };
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => flowService.CreateFlowAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"The Trigger ID: {triggerId} is invalid.", exception.Message);
  }

  [Fact]
  public async Task UpdateFlowAsync_CalledWithValidFlow_ShouldReturnsFlow()
  {
    // Arrange
    int id = 1;
    var expected = flows.FirstOrDefault(t => t.Id == id);
    var update = new FlowUpdateBusinessModel {
      Name = "Test Flow Edited",
      FlowDetails = [],
    };
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await flowService.UpdateFlowAsync(id, update);

    // Assert
    Assert.True(actual);
    Assert.Equal(expected!.Name, update.Name);
  }

  [Fact]
  public async Task UpdateFlowAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = flows.Count + 1;
    var update = new FlowUpdateBusinessModel {
      Name = "Test Flow Edited",
      FlowDetails = [],
    };
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => flowService.UpdateFlowAsync(id, update);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task TestDeleteFlowAsync_CalledWithValidId_ShouldReturnsTrue()
  {
    // Arrange
    int id = 1;
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await flowService.TestDeleteFlowAsync(id);

    // Assert
    Assert.True(actual);
  }

  [Fact]
  public async Task TestDeleteFlowAsync_CalledWithAutoTaskDepeendencies_ShouldThrowsValidationExpection()
  {
    // Arrange
    int id = 2;
    int depeendencies = 1;
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => flowService.TestDeleteFlowAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"This flow has been used by {depeendencies} running/waiting/template tasks.", exception.Message);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Flow")]
  [InlineData("Flow 1")]
  [InlineData("Flow 2")]
  [InlineData("AAA")]
  public async Task SearchFlowsAsync_CalledWithName_ShouldReturnsFlowsWithName(string name)
  {
    // Arrange
    var expected = flows.Where(t => t.Name!.Contains(name));
    var flowService = new FlowService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await flowService.SearchFlowsAsync(name);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => Assert.Contains(name, a.Name));
  }
}