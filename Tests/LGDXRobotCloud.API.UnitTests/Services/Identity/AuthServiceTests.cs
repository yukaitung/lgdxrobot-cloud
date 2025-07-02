using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Identity;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Identity;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Identity;

public class AuthServiceTests
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
    "role1",
    "role2"
  ];

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly Mock<IEmailService> mockEmailService = new();
  private readonly Mock<IOptionsSnapshot<LgdxRobotCloudSecretConfiguration>> mockSecretConfiguration = new();
  private readonly Mock<SignInManager<LgdxUser>> mockSignInManager;
  private readonly Mock<UserManager<LgdxUser>> mockUserManager;
  private readonly LgdxContext lgdxContext;
  private readonly LgdxRobotCloudSecretConfiguration lgdxRobotCloudSecretConfiguration = new();

  public AuthServiceTests()
  {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    mockUserManager = new Mock<UserManager<LgdxUser>>(new Mock<IUserStore<LgdxUser>>().Object, null, null, null, null, null, null, null, null);
    mockSignInManager = new Mock<SignInManager<LgdxUser>>(mockUserManager.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<LgdxUser>>(), null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    lgdxRobotCloudSecretConfiguration.LgdxUserJwtSecret = "12345678901234567890123456789012";
    mockSecretConfiguration.Setup(o => o.Value).Returns(lgdxRobotCloudSecretConfiguration);
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
  }

  [Fact]
  public async Task LoginAsync_CalledWithValidUsernameAndPassword_ShouldReturnsAccessToken()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.Success);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    var actual = await authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    Assert.NotNull(actual);
    Assert.NotEmpty(actual.AccessToken);
    Assert.NotEmpty(actual.RefreshToken);
    Assert.Equal(lgdxRobotCloudSecretConfiguration.LgdxUserAccessTokenExpiresMins, actual.ExpiresMins);
    Assert.False(actual.RequiresTwoFactor);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Never);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task LoginAsync_CalledWithValidUsernameAndPasswordRequiredTwoFactor_ShouldReturnsAccessToken()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.TwoFactorRequired);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    var actual = await authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    Assert.NotNull(actual);
    Assert.Empty(actual.AccessToken);
    Assert.Empty(actual.RefreshToken);
    Assert.Equal(0, actual.ExpiresMins);
    Assert.True(actual.RequiresTwoFactor);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Never);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_CalledWithValidUsernameAndPasswordTwoFactor_ShouldReturnsAccessToken()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test",
      TwoFactorCode = "123456"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.TwoFactorRequired);
    mockSignInManager.Setup(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false)).ReturnsAsync(SignInResult.Success);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    var actual = await authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    Assert.NotNull(actual);
    Assert.NotNull(actual.AccessToken);
    Assert.NotNull(actual.RefreshToken);
    Assert.Equal(lgdxRobotCloudSecretConfiguration.LgdxUserAccessTokenExpiresMins, actual.ExpiresMins);
    Assert.False(actual.RequiresTwoFactor);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task LoginAsync_CalledWithValidUsernameAndPasswordRecoveryCode_ShouldReturnsAccessToken()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test",
      TwoFactorRecoveryCode = "123456"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.TwoFactorRequired);
    mockSignInManager.Setup(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>())).ReturnsAsync(SignInResult.Success);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    var actual = await authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    Assert.NotNull(actual);
    Assert.NotNull(actual.AccessToken);
    Assert.NotNull(actual.RefreshToken);
    Assert.Equal(lgdxRobotCloudSecretConfiguration.LgdxUserAccessTokenExpiresMins, actual.ExpiresMins);
    Assert.False(actual.RequiresTwoFactor);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Never);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task LoginAsync_CalledWithInValidUsernameAndPassword_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.Failed);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The username or password is invalid.", exception.Message);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Never);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_CalledWithLockedUsernameAndPassword_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.LockedOut);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The account is locked out.", exception.Message);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Never);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_CalledWithInvalidTwoFactorCode_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test",
      TwoFactorCode = "123456"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.TwoFactorRequired);
    mockSignInManager.Setup(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false)).ReturnsAsync(SignInResult.Failed);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The 2FA code is invalid.", exception.Message);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_CalledWithInValidRecoveryCode_ShouldReturnsAccessToken()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test",
      TwoFactorRecoveryCode = "123456"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.TwoFactorRequired);
    mockSignInManager.Setup(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>())).ReturnsAsync(SignInResult.Failed);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The recovery code is invalid.", exception.Message);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Never);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_CalledWithInvalidUser_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The username or password is invalid.", exception.Message);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_CalledWithUserUpdate_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.Success);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Failed());
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.LoginAsync(loginRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockSignInManager.Verify(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true), Times.Once);
    mockSignInManager.Verify(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), false, false), Times.Never);
    mockSignInManager.Verify(m => m.TwoFactorRecoveryCodeSignInAsync(It.IsAny<string>()), Times.Never);
    mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task ForgotPasswordAsync_CalledWithValidEmail_ShouldSendEmail()
  {
    // Arrange
    var forgotPasswordRequestBusinessModel = new ForgotPasswordRequestBusinessModel {
      Email = "test@example.com"
    };
    mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<LgdxUser>())).ReturnsAsync("123456");
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    await authService.ForgotPasswordAsync(forgotPasswordRequestBusinessModel);

    // Assert
    mockUserManager.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GeneratePasswordResetTokenAsync(It.IsAny<LgdxUser>()), Times.Once);
    mockEmailService.Verify(m => m.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
  }

  [Fact]
  public async Task ForgotPasswordAsync_CalledWithInvalidEmail_ShouldNotSendEmail()
  {
    // Arrange
    var forgotPasswordRequestBusinessModel = new ForgotPasswordRequestBusinessModel {
      Email = "test@example.com"
    };
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    await authService.ForgotPasswordAsync(forgotPasswordRequestBusinessModel);

    // Assert
    mockUserManager.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.GeneratePasswordResetTokenAsync(It.IsAny<LgdxUser>()), Times.Never);
    mockEmailService.Verify(m => m.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task ResetPasswordAsync_CalledWithValidToken_ShouldSendsEmail()
  {
    // Arrange
    var resetPasswordRequestBusinessModel = new ResetPasswordRequestBusinessModel {
      Token = "123456",
      NewPassword = "test",
      Email = "test@example.com"
    };
    mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.ResetPasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    await authService.ResetPasswordAsync(resetPasswordRequestBusinessModel);

    // Assert
    mockUserManager.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.ResetPasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    mockEmailService.Verify(m => m.SendPasswordUpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
  }

  [Fact]
  public async Task ResetPasswordAsync_CalledWithInvalidEmail_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var resetPasswordRequestBusinessModel = new ResetPasswordRequestBusinessModel {
      Token = "123456",
      NewPassword = "test",
      Email = "test@example.com"
    };
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.ResetPasswordAsync(resetPasswordRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    mockUserManager.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.ResetPasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    mockEmailService.Verify(m => m.SendPasswordUpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task ResetPasswordAsync_CalledWithInvalidToken_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var resetPasswordRequestBusinessModel = new ResetPasswordRequestBusinessModel {
      Token = "123456",
      NewPassword = "test",
      Email = "test@example.com"
    };
    mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.ResetPasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.ResetPasswordAsync(resetPasswordRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.ResetPasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    mockEmailService.Verify(m => m.SendPasswordUpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task RefreshTokenAsync_Called_ShouldReturnsToken()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.Success);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);
    var token = await authService.LoginAsync(loginRequestBusinessModel);
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new LgdxUser{
      Id = user.Id.ToString(),
      Name = user.Name,
      UserName = user.UserName,
      Email = user.Email,
      NormalizedUserName = user.NormalizedUserName,
      NormalizedEmail = user.NormalizedEmail,
      AccessFailedCount = user.AccessFailedCount,
      RefreshTokenHash = LgdxHelper.GenerateSha256Hash(token.RefreshToken)
    });
    var refreshTokenRequestBusinessModel = new RefreshTokenRequestBusinessModel {
      RefreshToken = token.RefreshToken
    };

    // Act
    var actual = await authService.RefreshTokenAsync(refreshTokenRequestBusinessModel);
    
    // Assert
    Assert.NotEmpty(actual.AccessToken);
    Assert.NotEmpty(actual.RefreshToken);
    Assert.Equal(lgdxRobotCloudSecretConfiguration.LgdxUserAccessTokenExpiresMins, actual.ExpiresMins);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.AtLeastOnce);
  }

  [Fact]
  public async Task RefreshTokenAsync_CalledWithInvalidUserId_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.Success);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);
    var token = await authService.LoginAsync(loginRequestBusinessModel);
    var refreshTokenRequestBusinessModel = new RefreshTokenRequestBusinessModel {
      RefreshToken = token.RefreshToken
    };

    // Act
    Task act() => authService.RefreshTokenAsync(refreshTokenRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("User not found.", exception.Message);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task RefreshTokenAsync_CalledWithUsedRefreshToken_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.Success);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);
    var token = await authService.LoginAsync(loginRequestBusinessModel);
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new LgdxUser{
      Id = user.Id.ToString(),
      Name = user.Name,
      UserName = user.UserName,
      Email = user.Email,
      NormalizedUserName = user.NormalizedUserName,
      NormalizedEmail = user.NormalizedEmail,
      AccessFailedCount = user.AccessFailedCount,
      RefreshTokenHash = "12345678901234567890123456789012"
    });
    var refreshTokenRequestBusinessModel = new RefreshTokenRequestBusinessModel {
      RefreshToken = token.RefreshToken
    };

    // Act
    Task act() => authService.RefreshTokenAsync(refreshTokenRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("The refresh token is used.", exception.Message);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.Once);
  }

  [Fact]
  public async Task RefreshTokenAsync_CalledWithFailedUpdate_ShouldThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var loginRequestBusinessModel = new LoginRequestBusinessModel {
      Username = "test",
      Password = "test"
    };
    mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockSignInManager.Setup(m => m.PasswordSignInAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), false, true)).ReturnsAsync(SignInResult.Success);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Success);
    mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<LgdxUser>())).ReturnsAsync(roles);
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);
    var token = await authService.LoginAsync(loginRequestBusinessModel);
    mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxUser>())).ReturnsAsync(IdentityResult.Failed());
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new LgdxUser{
      Id = user.Id.ToString(),
      Name = user.Name,
      UserName = user.UserName,
      Email = user.Email,
      NormalizedUserName = user.NormalizedUserName,
      NormalizedEmail = user.NormalizedEmail,
      AccessFailedCount = user.AccessFailedCount,
      RefreshTokenHash = LgdxHelper.GenerateSha256Hash(token.RefreshToken)
    });
    var refreshTokenRequestBusinessModel = new RefreshTokenRequestBusinessModel {
      RefreshToken = token.RefreshToken
    };

    // Act
    Task act() => authService.RefreshTokenAsync(refreshTokenRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxUser>()), Times.AtLeastOnce);
  }

  [Fact]
  public async Task UpdatePasswordAsync_CalledWithValidUserIdUpdate_ShouldReturnsTrue()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.ChangePasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
    var updatePasswordRequestBusinessModel = new UpdatePasswordRequestBusinessModel {
      CurrentPassword = "test",
      NewPassword = "test"
    };
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    var actual = await authService.UpdatePasswordAsync(user.Id, updatePasswordRequestBusinessModel);

    // Assert
    Assert.True(actual);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.ChangePasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    mockEmailService.Verify(m => m.SendPasswordUpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
  }

  [Fact]
  public async Task UpdatePasswordAsync_CalledWithInvalidUserId_ShouldThrowsLgdxValidation400Expection()
  {
    // Arrange
    var updatePasswordRequestBusinessModel = new UpdatePasswordRequestBusinessModel {
      CurrentPassword = "test",
      NewPassword = "test"
    };
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.UpdatePasswordAsync(user.Id, updatePasswordRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.ChangePasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    mockEmailService.Verify(m => m.SendPasswordUpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task UpdatePasswordAsync_CalledWithInvaliInvalidCurrentPassword_ShouldThrowsLgdxIdentity400Expection()
  {
    // Arrange
    mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    mockUserManager.Setup(m => m.ChangePasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());
    var updatePasswordRequestBusinessModel = new UpdatePasswordRequestBusinessModel {
      CurrentPassword = "test",
      NewPassword = "test"
    };
    var authService = new AuthService(mockActivityLogService.Object, lgdxContext, mockEmailService.Object, mockSecretConfiguration.Object, mockSignInManager.Object, mockUserManager.Object);

    // Act
    Task act() => authService.UpdatePasswordAsync(user.Id, updatePasswordRequestBusinessModel);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockUserManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockUserManager.Verify(m => m.ChangePasswordAsync(It.IsAny<LgdxUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    mockEmailService.Verify(m => m.SendPasswordUpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }
}