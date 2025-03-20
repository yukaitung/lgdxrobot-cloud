using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.API.UnitTests.Services.Navigation;

public class WaypointServiceTests
{
  private readonly List<Waypoint> waypoints = [
    new()
    {
      Id = 1,
      Name = "Waypoint 1",
      X = 43,
      Y = 56,
      Rotation = 0.233,
      RealmId = 1,
      IsParking = true,
      HasCharger = true,
      IsReserved = true
    },
    new()
    {
      Id = 2,
      Name = "Waypoint 2",
      X = 4,
      Y = 45,
      Rotation = 0.56,
      RealmId = 1,
      IsParking = true,
      HasCharger = false,
      IsReserved = false
    },
    new()
    {
      Id = 3,
      Name = "Waypoint 3",
      X = 12,
      Y = 34,
      Rotation = 0.12,
      RealmId = 2,
      IsParking = false,
      HasCharger = true,
      IsReserved = true
    }
  ];

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
    }
  ];

  private readonly List<AutoTask> autoTasks = [
    new() 
    {
      Id = 1,
      Name = "Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      CurrentProgressId = (int)ProgressState.Waiting,
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
      WaypointId = 2,
    }
  ];

  private readonly LgdxContext lgdxContext;

  public WaypointServiceTests()
  {
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<Realm>().AddRange(realms);
    lgdxContext.Set<Waypoint>().AddRange(waypoints);
    lgdxContext.Set<AutoTask>().AddRange(autoTasks);
    lgdxContext.Set<AutoTaskDetail>().AddRange(autoTaskDetails);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData(null, "")]
  [InlineData(1, "Waypoint")]
  [InlineData(2, "Waypoint")]
  [InlineData(1, "Waypoint 1")]
  [InlineData(2, "Waypoint 1")]
  [InlineData(1, "123")]
  public async Task GetWaypointsAsync_CalledWithWaypointName_ShouldReturnsWaypoints(int? realmId, string waypointName)
  {
    // Arrange
    var expected = waypoints.Where(w => realmId == null || w.RealmId == realmId).Where(w => w.Name.Contains(waypointName)).ToList();
    var waypointService = new WaypointService(lgdxContext);

    // Act
    var (actual, _) = await waypointService.GetWaypointsAsync(realmId, waypointName, 1, waypoints.Count);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      var e = expected.FirstOrDefault(e => e.Id == a.Id);
      Assert.NotNull(e);
      Assert.Equal(e.Name, a.Name);
      Assert.Equal(e.X, a.X);
      Assert.Equal(e.Y, a.Y);
      Assert.Equal(e.Rotation, a.Rotation);
      Assert.Equal(e.RealmId, a.RealmId);
      Assert.Equal(e.IsParking, a.IsParking);
      Assert.Equal(e.HasCharger, a.HasCharger);
      Assert.Equal(e.IsReserved, a.IsReserved);
    });
  }

  [Fact]
  public async Task GetWaypointAsync_CalledWithValidId_ShouldReturnsWaypoint()
  {
    // Arrange
    var expected = waypoints.Where(w => w.Id == 1).FirstOrDefault();
    var waypointService = new WaypointService(lgdxContext);

    // Act
    var actual = await waypointService.GetWaypointAsync(1);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected!.Name, actual.Name);
    Assert.Equal(expected.X, actual.X);
    Assert.Equal(expected.Y, actual.Y);
    Assert.Equal(expected.Rotation, actual.Rotation);
    Assert.Equal(expected.RealmId, actual.RealmId);
    Assert.Equal(expected.IsParking, actual.IsParking);
    Assert.Equal(expected.HasCharger, actual.HasCharger);
    Assert.Equal(expected.IsReserved, actual.IsReserved);
  }

  [Fact]
  public async Task GetWaypointAsync_CalledWithInvalidId_ShouldThrowsNotFoundException()
  {
    // Arrange
    var waypointService = new WaypointService(lgdxContext);

    // Act
    Task act() => waypointService.GetWaypointAsync(waypoints.Count + 1);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task CreateWaypointAsync_CalledWithValidWaypoint_ShouldReturnsWaypoint()
  {
    // Arrange
    var expected = new WaypointCreateBusinessModel {
      Name = "Test Waypoint",
      RealmId = 1,
      X = 123,
      Y = 456,
      Rotation = 0.123,
      IsParking = true,
      HasCharger = true,
      IsReserved = true
    };
    var waypointService = new WaypointService(lgdxContext);

    // Act
    var actual = await waypointService.CreateWaypointAsync(expected);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(expected.Name, actual.Name);
    Assert.Equal(expected.X, actual.X);
    Assert.Equal(expected.Y, actual.Y);
    Assert.Equal(expected.Rotation, actual.Rotation);
    Assert.Equal(expected.RealmId, actual.RealmId);
    Assert.Equal(expected.IsParking, actual.IsParking);
    Assert.Equal(expected.HasCharger, actual.HasCharger);
    Assert.Equal(expected.IsReserved, actual.IsReserved);
  }

  [Fact]
  public async Task CreateWaypointAsync_CalledWithInvalidRealm_ShouldThrowsValidationException()
  {
    // Arrange
    var expected = new WaypointCreateBusinessModel {
      Name = "Test Waypoint",
      RealmId = realms.Count + 1,
      X = 123,
      Y = 456,
      Rotation = 0.123,
      IsParking = true,
      HasCharger = true,
      IsReserved = true
    };
    var waypointService = new WaypointService(lgdxContext);

    // Act
    Task act() => waypointService.CreateWaypointAsync(expected);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
  }

  [Fact]
  public async Task TestDeleteWaypointAsync_CalledWithValidId_ShouldReturnsTrue()
  {
    // Arrange
    var waypointService = new WaypointService(lgdxContext);

    // Act
    var actual = await waypointService.TestDeleteWaypointAsync(1);

    // Assert
    Assert.True(actual);
  }

  [Fact]
  public async Task TestDeleteWaypointAsync_CalledWithAutoTaskDependency_ShouldThrowsValidationException()
  {
    // Arrange
    var dependencies = 1;
    var waypointService = new WaypointService(lgdxContext);

    // Act
    Task act() => waypointService.TestDeleteWaypointAsync(2);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal($"This waypoint has been used by {dependencies} running/waiting/template tasks.", exception.Message);
  }

  [Theory]
  [InlineData("")]
  [InlineData("Realm")]
  [InlineData("Realm 1")]
  [InlineData("123")]
  public async Task SearchWaypointsAsync_CalledWithWaypointName_ShouldReturnsWaypoints(string name)
  {
    // Arrange
    var expected = waypoints.Where(w => w.Name.Contains(name)).Where(w => w.RealmId == 1);
    var waypointService = new WaypointService(lgdxContext);

    // Act
    var actual = await waypointService.SearchWaypointsAsync(1, name);

    // Assert
    Assert.Equal(expected.Count(), actual.Count());
    Assert.All(actual, a => {
      Assert.Contains(name, a.Name);
    });
  }
}