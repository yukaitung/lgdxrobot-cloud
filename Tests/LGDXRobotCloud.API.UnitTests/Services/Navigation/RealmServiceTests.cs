using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.API.UnitTests.Services.Navigation;

public class RealmServiceTests
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");

  private readonly List<Realm> realms = [
    new ()
    {
      Id = 1,
      Name = "Realm 1",
      Image = [],
      Resolution = 1,
      OriginX = 0,
      OriginY = 0,
      OriginRotation = 0
    },
    new ()
    {
      Id = 2,
      Name = "Realm 2",
      Image = [],
      Resolution = 1,
      OriginX = 0,
      OriginY = 0,
      OriginRotation = 0
    },
    new ()
    {
      Id = 3,
      Name = "Realm 3",
      Image = [],
      Resolution = 1,
      OriginX = 0,
      OriginY = 0,
      OriginRotation = 0
    },
    new ()
    {
      Id = 4,
      Name = "Realm 4",
      Image = [],
      Resolution = 1,
      OriginX = 0,
      OriginY = 0,
      OriginRotation = 0
    }
  ];

  private readonly List<Robot> robots = [
    new() {
      Id = RobotGuid,
      Name = "Test Robot 1",
      RealmId = 2
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
      RealmId = 3
    }
  ];

  private readonly List<AutoTask> autoTasks = [
    new() 
    {
      Id = 1,
      Name = "Waiting Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 4,
      CurrentProgressId = (int)ProgressState.Waiting,
    }
  ];

  private readonly LgdxContext lgdxContext;

  public RealmServiceTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<Realm>().AddRange(realms);
    lgdxContext.Set<Robot>().AddRange(robots);
    lgdxContext.Set<Waypoint>().AddRange(waypoints);
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData("")]
  [InlineData("Realm")]
  [InlineData("Realm 1")]
  [InlineData("123")]
  public async Task GetRealmsAsync_Called_ShouldReturnsRealms(string realmName)
  {
    // Arrange
    var expected = realms.Where(r => r.Name.Contains(realmName));
    var realmService = new RealmService(lgdxContext);

    // Act
    var (actual, _) = await realmService.GetRealmsAsync(realmName, 1, realms.Count);
    
    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a=> {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.Equal(e.Description, a.Description);
      Assert.Equal(e.Resolution, a.Resolution);
    });
  }

  [Fact]
  public async Task GetRealmAsync_CalledWithValidId_ShouldReturnsRealm()
  {
    // Arrange
    int id = 1;
    var expected = realms.Where(r => r.Id == id).FirstOrDefault();
    var realmService = new RealmService(lgdxContext);

    // Act
    var actual = await realmService.GetRealmAsync(id);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Id, actual.Id);
    Assert.Equal(expected!.Name, actual.Name);
    Assert.Equal(expected!.Description, actual.Description);
    Assert.Equal(expected!.Resolution, actual.Resolution);
    Assert.Equal(expected!.OriginX, actual.OriginX);
    Assert.Equal(expected!.OriginY, actual.OriginY);
    Assert.Equal(expected!.OriginRotation, actual.OriginRotation);
  }

  [Fact]
  public async Task GetRealmAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    var id = realms.Count + 1;
    var realmService = new RealmService(lgdxContext);

    // Act
    Task act() => realmService.GetRealmAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task GetDefaultRealmAsync_Called_ShouldReturnsFirstRealm()
  {
    // Arrange
    var expected = realms.FirstOrDefault();
    var realmService = new RealmService(lgdxContext);

    // Act
    var actual = await realmService.GetDefaultRealmAsync();

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Id, actual.Id);
    Assert.Equal(expected!.Name, actual.Name);
    Assert.Equal(expected!.Description, actual.Description);
    Assert.Equal(expected!.Resolution, actual.Resolution);
    Assert.Equal(expected!.OriginX, actual.OriginX);
    Assert.Equal(expected!.OriginY, actual.OriginY);
    Assert.Equal(expected!.OriginRotation, actual.OriginRotation);
  }

  [Fact]
  public async Task CreateRealmAsync_CalledWithValidRealm_ShouldReturnsRealm()
  {
    // Arrange
    var expected = new RealmCreateBusinessModel {
      Name = "Test Realm",
      Description = "Description",
      HasWaypointsTrafficControl = false,
      Image = "",
      Resolution = 0.05,
      OriginX = 0.1,
      OriginY = 0.2,
      OriginRotation = 3.14
    };
    var realmService = new RealmService(lgdxContext);

    // Act
    var actual = await realmService.CreateRealmAsync(expected);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected.Name, actual.Name);
    Assert.Equal(expected.Description, actual.Description);
    Assert.Equal(expected.Resolution, actual.Resolution);
    Assert.Equal(expected.OriginX, actual.OriginX);
    Assert.Equal(expected.OriginY, actual.OriginY);
    Assert.Equal(expected.OriginRotation, actual.OriginRotation);
  }
  
  [Fact]
  public async Task TestDeleteRealmAsync_CalledWithValidRealmId_ShouldReturnsTrue()
  {
    // Arrange
    int id = 1;
    var realmService = new RealmService(lgdxContext);

    // Act
    var actual = await realmService.TestDeleteRealmAsync(id);

    // Assert
    Assert.True(actual);
  }

  [Fact]
  public async Task TestDeleteRealmAsync_CalledWithRobotDependencies_ShouldThrowsValidationException()
  {
    // Arrange
    int dependencies = 1;
    int id = 2;
    var realmService = new RealmService(lgdxContext);

    // Act
    Task act() => realmService.TestDeleteRealmAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"This realm has been used by {dependencies} robots.", exception.Message);
  }

  [Fact]
  public async Task TestDeleteRealmAsync_CalledWithWaypointDependencies_ShouldThrowsValidationException()
  {
    // Arrange
    int dependencies = 1;
    int id = 3;
    var realmService = new RealmService(lgdxContext);

    // Act
    Task act() => realmService.TestDeleteRealmAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"This realm has been used by {dependencies} waypoints.", exception.Message);
  }

  [Fact]
  public async Task TestDeleteRealmAsync_CalledWithAutoTaskDependencies_ShouldThrowsValidationException()
  {
    // Arrange
    int dependencies = 1;
    int id = 4;
    var realmService = new RealmService(lgdxContext);

    // Act
    Task act() => realmService.TestDeleteRealmAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"This realm has been used by {dependencies} running/waiting/template tasks.", exception.Message);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Realm")]
  [InlineData("Realm 1")]
  [InlineData("123")]
  public async Task SearchRealmsAsync_CalledWithName_ShouldReturnsRealmsWithName(string name)
  {
    // Arrange
    var expected = realms.Where(p => p.Name.Contains(name));
    var realmService = new RealmService(lgdxContext);

    // Act
    var actual = await realmService.SearchRealmsAsync(name);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      Assert.Contains(name, a.Name);
    });
  }
}