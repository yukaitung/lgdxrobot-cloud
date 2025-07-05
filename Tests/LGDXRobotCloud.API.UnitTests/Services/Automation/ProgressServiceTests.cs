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

public class ProgressServiceTests
{
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
    },
    new ()
    {
      Id = (int)ProgressState.Reserved + 1,
      Name = "Progress 1",
    },
    new ()
    {
      Id = (int)ProgressState.Reserved + 2,
      Name = "Progress 2",
    }
  ];

  private readonly List<Flow> flows = [
    new()
    {
      Id = 1,
      Name = "Flow 1"
    }
  ];

  private readonly List<FlowDetail> flowDetails = [
    new()
    {
      Id = 1,
      Order = 0,
      ProgressId = (int)ProgressState.Reserved + 2,
      AutoTaskNextControllerId = (int)AutoTaskNextController.Robot,
      FlowId = 1
    },
  ];

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly LgdxContext lgdxContext;

  public ProgressServiceTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<Flow>().AddRange(flows);
    lgdxContext.Set<FlowDetail>().AddRange(flowDetails);
    lgdxContext.Set<Progress>().AddRange(progresses);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData("")]
  [InlineData("Progress")]
  [InlineData("Progress 1")]
  [InlineData("Template")]
  [InlineData("Moving")]
  [InlineData("321")]
  public async Task GetProgressesAsync_CalledWithSystemProgresses_ShouldReturnsProgresses(string progressName)
  {
    // Arrange
    var expected = progresses.Where(p => p.Name!.Contains(progressName)).Where(p => p.System);
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    var (actual, _) = await progressService.GetProgressesAsync(progressName, 1, 10, true);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.True(e.System);
      Assert.Equal(e.Reserved, a.Reserved);
    });
  }

  [Theory]
  [InlineData("")]
  [InlineData("Progress")]
  [InlineData("Progress 1")]
  [InlineData("Template")]
  [InlineData("Moving")]
  [InlineData("321")]
  public async Task GetProgressesAsync_CalledWithNonSystemProgresses_ShouldReturnsProgresses(string progressName)
  {
    // Arrange
    var expected = progresses.Where(p => p.Name!.Contains(progressName)).Where(p => p.System == false);
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    var (actual, _) = await progressService.GetProgressesAsync(progressName, 1, 10, false);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.False(e.System);
      Assert.Equal(e.Reserved, a.Reserved);
    });
  }

  [Fact]
  public async Task GetProgressAsync_CalledWithValidId_ShouldReturnsProgress()
  {
    // Arrange
    int id = (int)ProgressState.Template;
    var expected = progresses.FirstOrDefault(p => p.Id == id);
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await progressService.GetProgressAsync(id);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Id, actual.Id);
    Assert.Equal(expected!.Name, actual.Name);
    Assert.Equal(expected!.System, actual.System);
    Assert.Equal(expected!.Reserved, actual.Reserved);
  }

  [Fact]
  public async Task GetProgressAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = 999;
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => progressService.GetProgressAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task CreateProgressAsync_CalledWithValidProgress_ShouldReturnsProgress()
  {
    // Arrange
    var expected = new ProgressCreateBusinessModel {
      Name = "Test Progress"
    };
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await progressService.CreateProgressAsync(expected);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected.Name, actual.Name);
    Assert.False(actual.System);
    Assert.False(actual.Reserved);
  }

  [Fact]
  public async Task UpdateProgressAsync_CalledWithalidProgress_ShouldReturnsTrue()
  {
    // Arrange
    int id = (int)ProgressState.Reserved + 1;
    var expected = progresses.FirstOrDefault(p => p.Id == id);
    var update = new ProgressUpdateBusinessModel {
      Name = "Progress 1 Updated"
    };
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await progressService.UpdateProgressAsync(id, update);

    // Assert
    Assert.True(actual);
    Assert.Equal(update.Name, expected!.Name);
  }

  [Fact]
  public async Task UpdateProgressAsync_CalledWithInvalidProgressId_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = 999;
    var update = new ProgressUpdateBusinessModel {
      Name = "Progress 1 Updated"
    };
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => progressService.UpdateProgressAsync(id, update);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task UpdateProgressAsync_CalledWithSystemProgress_ShouldThrowsValidationException()
  {
    // Arrange
    int id = (int)ProgressState.Template;
    var update = new ProgressUpdateBusinessModel {
      Name = "Progress 1 Updated"
    };
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => progressService.UpdateProgressAsync(id, update);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("Cannot update system progress.", exception.Message);
  }

  [Fact]
  public async Task TestDeleteProgressAsync_CalledWithProgress_ShouldReturnsTrue()
  {
    // Arrange
    int id = (int)ProgressState.Reserved + 1;
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await progressService.TestDeleteProgressAsync(id);

    // Assert
    Assert.True(actual);
  }

  [Fact]
  public async Task TestDeleteProgressAsync_CalledWithInvalidProgressId_ShouldThrowsNotFoundException()
  {
    // Arrange
    int id = 999;
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => progressService.TestDeleteProgressAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task TestDeleteProgressAsync_CalledWithSystemProgress_ShouldThrowsValidationException()
  {
    // Arrange
    int id = (int)ProgressState.Template;
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => progressService.TestDeleteProgressAsync(id);

    // Assert 
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
  }

  [Fact]
  public async Task TestDeleteProgressAsync_CalledWithFlowDependencies_ShouldThrowsValidationException()
  {
    // Arrange
    int dependencies = 1;
    int id = (int)ProgressState.Reserved + 2;
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    Task act() => progressService.TestDeleteProgressAsync(id);

    // Assert 
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"This progress has been used by {dependencies} details in flows.", exception.Message);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Progress")]
  [InlineData("Progress 1")]
  [InlineData("Template")]
  [InlineData("Starting")]
  [InlineData("123")]
  public async Task SearchProgressesAsync_CalledWithNameAndNonReserved_ShouldReturnsProgressesWithName(string name)
  {
    // Arrange
    var expected = progresses.Where(p => p.Name!.Contains(name)).Where(p => p.Reserved == false);
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await progressService.SearchProgressesAsync(name, false);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      Assert.Contains(name, a.Name);
    });
  }

  [Theory]
  [InlineData("")]
  [InlineData("Progress")]
  [InlineData("Progress 1")]
  [InlineData("Template")]
  [InlineData("Starting")]
  [InlineData("123")]
  public async Task SearchProgressesAsync_CalledWithNameAndReserved_ShouldReturnsProgressesWithName(string name)
  {
    // Arrange
    var expected = progresses.Where(p => p.Name!.Contains(name)).Where(p => p.Reserved == true);
    var progressService = new ProgressService(mockActivityLogService.Object, lgdxContext);

    // Act
    var actual = await progressService.SearchProgressesAsync(name, true);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      Assert.Contains(name, a.Name);
    });
  }
}