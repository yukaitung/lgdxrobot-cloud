using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Identity;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Identity;

public class CurrentUserServiceTests
{
  private readonly static LgdxUser user = new() {
    Id = "0195d431-5d7c-74af-8e19-084e8976f637",
    Name = "Test User",
    UserName = "test",
    Email = "test@example.com",
    NormalizedUserName = "TEST USER",
    NormalizedEmail = "TEST@EXAMPLE.COM",
    AccessFailedCount = 0
  };

  private readonly static List<string> roles = [
    "role1"
  ];

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly Mock<UserManager<LgdxUser>> mockUserManager;

  public CurrentUserServiceTests()
  {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    mockUserManager = new Mock<UserManager<LgdxUser>>(new Mock<IUserStore<LgdxUser>>().Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
  }

  [Fact]
  public async Task GetUserAsync_Called_ShouldReturnsUser()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);

    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    var actual = await currentUserService.GetUserAsync(user.Id);

    // Assert
    Assert.NotNull(actual);
    Assert.Equal(user.Id, actual.Id.ToString());
    Assert.Equal(user.Name, actual.Name);
    Assert.Equal(user.UserName, actual.UserName);
    Assert.Equal(user.Email, actual.Email);
    Assert.Equal(roles, actual.Roles);
    Assert.Equal(user.TwoFactorEnabled, actual.TwoFactorEnabled);
    Assert.Equal(user.AccessFailedCount, actual.AccessFailedCount);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task GetUserAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    Task act() => currentUserService.GetUserAsync("invalid");

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task UpdateUserAsync_CalledWithValidUserIdUpdate_ShouldReturnsTrue()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    var lgdxUserUpdateBusinessModel = new LgdxUserUpdateBusinessModel {
      Name = "test",
      Email = "test@example.com"
    };
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    var actual = await currentUserService.UpdateUserAsync(user.Id, lgdxUserUpdateBusinessModel);

    // Assert
    Assert.True(actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task UpdateUserAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var lgdxUserUpdateBusinessModel = new LgdxUserUpdateBusinessModel {
      Name = "test",
      Email = "test@example.com"
    };
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    Task act() => currentUserService.UpdateUserAsync(Guid.Empty.ToString(), lgdxUserUpdateBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task UpdateUserAsync_CalledWithInvalidUserUpdate_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Failed());
    var lgdxUserUpdateBusinessModel = new LgdxUserUpdateBusinessModel {
      Name = "test",
      Email = "test@example.com"
    };
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    Task act() => currentUserService.UpdateUserAsync(user.Id.ToString(), lgdxUserUpdateBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task InitiateTwoFactorAsync_Called_ShouldReturnsKey()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.GetAuthenticatorKeyAsync(It.IsAny<LgdxUser>())).ReturnsAsync("123456");
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    var actual = await currentUserService.InitiateTwoFactorAsync(user.Id.ToString());

    // Assert
    Assert.Equal("123456", actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GetAuthenticatorKeyAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task EnableTwoFactorAsync_CalledWithValidTwoFactorCode_ShouldReturnsTrue()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.VerifyTwoFactorTokenAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
    mockUserManager.Setup(m => m.SetTwoFactorEnabledAsync(It.IsAny<LgdxUser>(), It.IsAny<bool>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<LgdxUser>(), It.IsAny<int>())).ReturnsAsync(["123456"]);
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    var actual = await currentUserService.EnableTwoFactorAsync(user.Id.ToString(), "123456");

    // Assert
    Assert.Single(actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.VerifyTwoFactorTokenAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.SetTwoFactorEnabledAsync(It.IsAny<LgdxUser>(), It.IsAny<bool>()), Times.Once);
    mockUserManager.Verify(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<LgdxUser>(), It.IsAny<int>()), Times.Once);
  }

  [Fact]
  public async Task EnableTwoFactorAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    Task act() => currentUserService.EnableTwoFactorAsync(Guid.Empty.ToString(), "123456");

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.VerifyTwoFactorTokenAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.SetTwoFactorEnabledAsync(It.IsAny<LgdxUser>(), It.IsAny<bool>()), Times.Never);
    mockUserManager.Verify(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<LgdxUser>(), It.IsAny<int>()), Times.Never);
  }

  [Fact]
  public async Task EnableTwoFactorAsync_CalledWithMissingTwoFactorCode_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    Task act() => currentUserService.EnableTwoFactorAsync(user.Id.ToString(), string.Empty);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The 2FA code is required.", exception.Message);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.VerifyTwoFactorTokenAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.SetTwoFactorEnabledAsync(It.IsAny<LgdxUser>(), It.IsAny<bool>()), Times.Never);
    mockUserManager.Verify(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<LgdxUser>(), It.IsAny<int>()), Times.Never);
  }

  [Fact]
  public async Task EnableTwoFactorAsync_CalledWithInvalidTwoFactorCode_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange  
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.VerifyTwoFactorTokenAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    Task act() => currentUserService.EnableTwoFactorAsync(user.Id.ToString(), "123456");

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The 2FA code is invalid.", exception.Message);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.VerifyTwoFactorTokenAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.SetTwoFactorEnabledAsync(It.IsAny<LgdxUser>(), It.IsAny<bool>()), Times.Never);
    mockUserManager.Verify(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<LgdxUser>(), It.IsAny<int>()), Times.Never);
  }

  [Fact]
  public async Task ResetRecoveryCodesAsync_Called_ShouldReturnsTrue()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<LgdxUser>(), It.IsAny<int>())).ReturnsAsync(["123456"]);
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    var actual = await currentUserService.ResetRecoveryCodesAsync(user.Id.ToString());

    // Assert
    Assert.Single(actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<LgdxUser>(), It.IsAny<int>()), Times.Once);
  }

  [Fact]
  public async Task ResetRecoveryCodesAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    Task act() => currentUserService.ResetRecoveryCodesAsync(Guid.Empty.ToString());

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GenerateNewTwoFactorRecoveryCodesAsync(It.IsAny<LgdxUser>(), It.IsAny<int>()), Times.Never);
  }

  [Fact]
  public async Task DisableTwoFactorAsync_Called_ShouldReturnsTrue()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.ResetAuthenticatorKeyAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.SetTwoFactorEnabledAsync(It.IsAny<LgdxUser>(), It.IsAny<bool>())).ReturnsAsync(IdentityResult.Success);
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    var actual = await currentUserService.DisableTwoFactorAsync(user.Id.ToString());

    // Assert
    Assert.True(actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.ResetAuthenticatorKeyAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.SetTwoFactorEnabledAsync(It.IsAny<LgdxUser>(), It.IsAny<bool>()), Times.Once);
  }

  [Fact]
  public async Task DisableTwoFactorAsync_CalledWithInvalidUserId_ShouldThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var currentUserService = new CurrentUserService(mockActivityLogService.Object, mockUserManager.Object);

    // Act
    Task act() => currentUserService.DisableTwoFactorAsync(Guid.Empty.ToString());

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.ResetAuthenticatorKeyAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.SetTwoFactorEnabledAsync(It.IsAny<LgdxUser>(), It.IsAny<bool>()), Times.Never);
  }
}