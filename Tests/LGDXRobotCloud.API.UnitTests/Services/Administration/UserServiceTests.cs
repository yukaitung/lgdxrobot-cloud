using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Administration;

public class UserServiceTests
{
  private readonly static Guid User1Id = Guid.Parse("0195d431-5d7c-74af-8e19-084e8976f637");
  private readonly static Guid User2Id = Guid.Parse("0195d431-e250-7924-b71c-ee5ba0607f03");
  private readonly static Guid User3Id = Guid.Parse("01942323-e76a-7ce8-a4f9-d550527ffe4e");

  private readonly List<LgdxUser> users = [
    new LgdxUser {
      Id = User1Id.ToString(),
      Name = "User 1",
      UserName = "user1",
      Email = "user1@example.com",
      NormalizedUserName = "USER 1",
      NormalizedEmail = "USER1@EXAMPLE.COM",
    },
    new LgdxUser {
      Id = User2Id.ToString(),
      Name = "User 2",
      UserName = "user2",
      Email = "user2@example.com",
      NormalizedUserName = "USER 2",
      NormalizedEmail = "USER2@EXAMPLE.COM",
    },
    new LgdxUser {
      Id = User3Id.ToString(),
      Name = "Manager 1",
      UserName = "manager1",
      Email = "manager1@example.com",
      NormalizedUserName = "MANAGER 1",
      NormalizedEmail = "MANAGER1@EXAMPLE.COM",
    }
  ];

  private readonly Mock<IEmailService> mockEmailService = new();
  private readonly Mock<UserManager<LgdxUser>> mockUserManager;
  private readonly LgdxContext lgdxContext;

  public UserServiceTests()
  {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    mockUserManager = new Mock<UserManager<LgdxUser>>(new Mock<IUserStore<LgdxUser>>().Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<LgdxUser>().AddRange(users);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData("")]
  [InlineData("USER")]
  [InlineData("USER 1")]
  [InlineData("AAA")]
  public async Task GetUsersAsync_CalledWithName_ShouldReturnsUsersWithName(string name)
  {
    // Arrange
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);
    var expected = users.Where(u => u.NormalizedUserName!.Contains(name)).ToList();

    // Act
    var (actual, _) = await userService.GetUsersAsync(name, 0, 10);

    // Assert
    Assert.Equal(expected.Count, actual.Count());
    Assert.All(actual, u => {
      var expected = actual.FirstOrDefault(e => e.Id == u.Id);
      Assert.NotNull(expected);
      Assert.Equal(expected.Name, u.Name);
      Assert.Equal(expected.UserName, u.UserName);
      Assert.Equal(expected.TwoFactorEnabled, u.TwoFactorEnabled);
      Assert.Equal(expected.AccessFailedCount, u.AccessFailedCount);
    });
  }

  [Fact]
  public async Task GetUserAsync_CalledWithUserId_ShouldReturnsUser()
  {
    // Arrange
    var userId = User1Id;
    mockUserManager.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(users.Where(u => u.Id == userId.ToString()).FirstOrDefault());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);
    var expected = users.FirstOrDefault(u => u.Id == User1Id.ToString());

    // Act
    var actual = await userService.GetUserAsync(User1Id);

    // Assert
    Assert.Equal(expected!.Id, actual.Id.ToString());
    Assert.Equal(expected.Name, actual.Name);
    Assert.Equal(expected.UserName, actual.UserName);
    Assert.Equal(expected.Email, actual.Email);
    Assert.Equal(expected.TwoFactorEnabled, actual.TwoFactorEnabled);
    Assert.Equal(expected.AccessFailedCount, actual.AccessFailedCount);
  }

  [Fact]
  public async Task GetUserAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var userId = Guid.Empty;
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.GetUserAsync(userId);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task CreateUserAsync_CalledWithUser_ShouldReturnsUser()
  {
    var user = new LgdxUserCreateAdminBusinessModel {
      Name = "Test User",
      Email = "test@example.com",
      UserName = "test",
      Password = "test",
      Roles = ["role1"]
    };
    mockUserManager.Setup(m => m.CreateAsync(It.IsAny<LgdxUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    var actual = await userService.CreateUserAsync(user);

    // Assert
    Assert.Equal(user.Name, actual.Name);
    Assert.Equal(user.Email, actual.Email);
    Assert.Equal(user.UserName, actual.UserName);
    Assert.Equal(user.Roles.First(), actual.Roles.First());
    mockUserManager.Verify(m => m.CreateAsync(It.IsAny<LgdxUser>(), It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    mockEmailService.Verify(m => m.SendWellcomePasswordSetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    mockEmailService.Verify(m => m.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
  }

  [Fact]
  public async Task CreateUserAsync_CalledWithUserNoPassword_ShouldReturnsUser()
  {
    var user = new LgdxUserCreateAdminBusinessModel {
      Name = "Test User",
      Email = "test@example.com",
      UserName = "test",
      Roles = ["role1"]
    };
    mockUserManager.Setup(m => m.CreateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    var actual = await userService.CreateUserAsync(user);

    // Assert
    Assert.Equal(user.Name, actual.Name);
    Assert.Equal(user.Email, actual.Email);
    Assert.Equal(user.UserName, actual.UserName);
    Assert.Equal(user.Roles.First(), actual.Roles.First());
    mockUserManager.Verify(m => m.CreateAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    mockEmailService.Verify(m => m.SendWellcomePasswordSetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    mockEmailService.Verify(m => m.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task CreateUserAsync_CalledWithFailingCreateAsync_ShouldThrowsLgdxIdentity400Expection()
  {
    var user = new LgdxUserCreateAdminBusinessModel {
      Name = "Test User",
      Email = "test@example.com",
      UserName = "test",
      Password = "test",
      Roles = ["role1"]
    };
    mockUserManager.Setup(m => m.CreateAsync(It.IsAny<LgdxUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.CreateUserAsync(user);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.CreateAsync(It.IsAny<LgdxUser>(), It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    mockEmailService.Verify(m => m.SendWellcomePasswordSetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    mockEmailService.Verify(m => m.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task CreateUserAsync_CalledWithFailingCreateAsyncNoPassword_ShouldThrowsLgdxIdentity400Expection()
  {
    var user = new LgdxUserCreateAdminBusinessModel {
      Name = "Test User",
      Email = "test@example.com",
      UserName = "test",
      Roles = ["role1"]
    };
    mockUserManager.Setup(m => m.CreateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Failed());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.CreateUserAsync(user);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.CreateAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    mockEmailService.Verify(m => m.SendWellcomePasswordSetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    mockEmailService.Verify(m => m.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task CreateUserAsync_CalledWithFailingAddToRolesAsync_ShouldThrowsLgdxIdentity400Expection()
  {
    var user = new LgdxUserCreateAdminBusinessModel {
      Name = "Test User",
      Email = "test@example.com",
      UserName = "test",
      Password = "test",
      Roles = ["role1"]
    };
    mockUserManager.Setup(m => m.CreateAsync(It.IsAny<LgdxUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Failed());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.CreateUserAsync(user);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.CreateAsync(It.IsAny<LgdxUser>(), It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    mockEmailService.Verify(m => m.SendWellcomePasswordSetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    mockEmailService.Verify(m => m.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task UpdateUserAsync_CalledWithUserId_ShouldReturnsTrue()
  {
    // Arrange
    var id = User1Id;
    var user = new LgdxUserUpdateAdminBusinessModel {
      Name = "Test User",
      UserName = "test",
      Email = "test@example.com",
      Roles = ["role2"]
    };
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(["role1"]);
    mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.RemoveFromRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    var actual = await userService.UpdateUserAsync(User1Id, user);

    // Assert
    Assert.True(actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    mockUserManager.Verify(m => m.RemoveFromRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
  }

  [Fact]
  public async Task UpdateUserAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var id = Guid.Empty;
    var user = new LgdxUserUpdateAdminBusinessModel {
      Name = "Test User",
      UserName = "test",
      Email = "test@example.com",
      Roles = ["role2"]
    };
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.UpdateUserAsync(id, user);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    mockUserManager.Verify(m => m.RemoveFromRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
  }

  [Fact]
  public async Task UpdateUserAsync_CalledWithFailingUpdateAsync_ShouldThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var id = User1Id;
    var user = new LgdxUserUpdateAdminBusinessModel {
      Name = "Test User",
      UserName = "test",
      Email = "test@example.com",
      Roles = ["role2"]
    };
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Failed());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.UpdateUserAsync(id, user);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    mockUserManager.Verify(m => m.RemoveFromRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
  }

  [Fact]
  public async Task UpdateUserAsync_CalledWithFailingAddToRoles_ShouldThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var id = User1Id;
    var user = new LgdxUserUpdateAdminBusinessModel {
      Name = "Test User",
      UserName = "test",
      Email = "test@example.com",
      Roles = ["role2"]
    };
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(["role1"]);
    mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Failed());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.UpdateUserAsync(id, user);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    mockUserManager.Verify(m => m.RemoveFromRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
  }

  [Fact]
  public async Task UpdateUserAsync_CalledWithFailingRemoveFromRoles_ShouldThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var id = User1Id;
    var user = new LgdxUserUpdateAdminBusinessModel {
      Name = "Test User",
      UserName = "test",
      Email = "test@example.com",
      Roles = ["role2"]
    };
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(["role1"]);
    mockUserManager.Setup(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.RemoveFromRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Failed());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.UpdateUserAsync(id, user);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    mockUserManager.Verify(m => m.RemoveFromRolesAsync(It.IsAny<LgdxUser>(), It.IsAny<IEnumerable<string>>()), Times.Once);
  }

  [Fact]  
  public async Task UnlockUserAsync_CalledWithUserId_ShouldReturnsTrue()
  {
    // Arrange
    var id = User1Id;
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    var actual = await userService.UnlockUserAsync(User1Id);

    // Assert
    Assert.True(actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task UnlockUserAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var id = Guid.Empty;
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.UnlockUserAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task UnlockUserAsync_CalledWithFailingUpdate_ShouldThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var id = User1Id;
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Failed());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.UnlockUserAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task DeleteUserAsync_CalledWithUserId_ShouldReturnsTrue()
  {
    // Arrange
    var id = User1Id;
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    mockUserManager.Setup(m => m.DeleteAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    var actual = await userService.DeleteUserAsync(User1Id, Guid.Empty.ToString());

    // Assert
    Assert.True(actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<LgdxUser>()), Times.Once);
  }
  
  [Fact]
  public async Task DeleteUserAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var id = Guid.Empty;
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.DeleteUserAsync(id, Guid.Empty.ToString());

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task DeleteUserAsync_CalledWithSameOperatorId_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var id = User1Id;
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.DeleteUserAsync(id, id.ToString());

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("Cannot delete yourself.", exception.Message);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task DeleteUserAsync_CalledWithFailingDelete_ShouldThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var id = User1Id;
    mockUserManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(users.Where(u => u.Id == id.ToString()).FirstOrDefault());
    mockUserManager.Setup(m => m.DeleteAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Failed());
    var userService = new UserService(mockEmailService.Object, mockUserManager.Object, lgdxContext);

    // Act
    Task act() => userService.DeleteUserAsync(id, Guid.Empty.ToString());

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<LgdxUser>()), Times.Once);
  }
}