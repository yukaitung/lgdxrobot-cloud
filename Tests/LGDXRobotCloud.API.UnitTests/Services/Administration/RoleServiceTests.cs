using System.Security.Claims;
using EntityFrameworkCore.Testing.Moq;
using LGDXRobotCloud.API.Exceptions;
using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Data.Models.Business.Administration;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Administration;

public class RoleServiceTests
{
  private readonly static Guid Role1Id = Guid.Parse("0195d431-5d7c-74af-8e19-084e8976f637");
  private readonly static Guid Role2Id = Guid.Parse("0195d431-e250-7924-b71c-ee5ba0607f03");
  private readonly static Guid Role3Id = Guid.Parse("01942323-e76a-7ce8-a4f9-d550527ffe4e");

  private readonly List<LgdxRole> roles = [
    new LgdxRole {
      Id = Role1Id.ToString(),
      Name = "Role 1",
      Description = "Test Description 1",
      NormalizedName = "ROLE 1",
    },
    new LgdxRole {
      Id = Role2Id.ToString(),
      Name = "Role 2",
      Description = "Test Description 2",
      NormalizedName = "ROLE 2",
    },
    new LgdxRole {
      Id = Role3Id.ToString(),
      Name = "Manager 1",
      Description = "Test Description 3",
      NormalizedName = "MANAGER 3",
    }
  ];

  private readonly List<IdentityRoleClaim<string>> roleClaims = [
    new IdentityRoleClaim<string> {
      ClaimType = "scope",
      ClaimValue = "scope1",
      RoleId = Role1Id.ToString()
    },
    new IdentityRoleClaim<string> {
      ClaimType = "scope",
      ClaimValue = "scope2",
      RoleId = Role2Id.ToString()
    },
    new IdentityRoleClaim<string> {
      ClaimType = "scope",
      ClaimValue = "scope3",
      RoleId = Role3Id.ToString()
    }
  ];

  private readonly Mock<RoleManager<LgdxRole>> mockRoleManager;
  private readonly LgdxContext lgdxContext;

  public RoleServiceTests()
  {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    mockRoleManager = new Mock<RoleManager<LgdxRole>>(new Mock<IRoleStore<LgdxRole>>().Object, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    lgdxContext = Create.MockedDbContextFor<LgdxContext>();
    lgdxContext.Set<LgdxRole>().AddRange(roles);
    lgdxContext.Set<IdentityRoleClaim<string>>().AddRange(roleClaims);
    lgdxContext.SaveChanges();
  }

  [Theory]
  [InlineData("")]
  [InlineData("ROLE")]
  [InlineData("ROLE 1")]
  [InlineData("AAA")]
  public async Task GetRolesAsync_CalledWithLgdxRole_ShouldReturnLgdxRoles(string roleName)
  {
    // Arrange
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);
    var expected = roles.Where(r => r.NormalizedName!.Contains(roleName)).ToList();

    // Act
    var (actual, _) = await roleService.GetRolesAsync(roleName, 0, 10);

    // Assert
    Assert.Equal(expected.Count, actual.Count());
    Assert.All(actual, r => {
      var expected = actual.FirstOrDefault(e => e.Id == r.Id);
      Assert.NotNull(expected);
      Assert.Equal(expected.Name, r.Name);
      Assert.Equal(expected.Description, r.Description);
    });
  }

  [Fact]
  public async Task GetRoleAsync_CalledWithRoleId_ShouldReturnLgdxRole()
  {
    // Arrange
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);
    var expected = roles.FirstOrDefault(r => r.Id == Role1Id.ToString());
    var expectedClaims = roleClaims.Where(c => c.RoleId == Role1Id.ToString()).FirstOrDefault();

    // Act
    var actual = await roleService.GetRoleAsync(Role1Id);

    // Assert
    Assert.Equal(expected!.Id, actual.Id.ToString());
    Assert.Equal(expected.Name, actual.Name);
    Assert.Equal(expected.Description, actual.Description);
    Assert.Single(actual.Scopes);
    Assert.Equal(expectedClaims!.ClaimValue, actual.Scopes.First());
  }

  [Fact]
  public async Task GetRoleAsync_CalledWithInvalidRoleId_ShouldReturnLgdxNotFound404Exception()
  {
    // Arrange
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.GetRoleAsync(Guid.NewGuid());

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
  }

  [Fact]
  public async Task CreateRoleAsync_CalledWithLgdxRole_ShouldReturnLgdxRole()
  {
    // Arrange
    mockRoleManager.Setup(m => m.CreateAsync(It.IsAny<LgdxRole>())).Returns(Task.FromResult(IdentityResult.Success));
    mockRoleManager.Setup(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>())).Returns(Task.FromResult(IdentityResult.Success));
    var role = new LgdxRoleCreateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope1"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    var actual = await roleService.CreateRoleAsync(role);

    // Assert
    Assert.Equal(role.Name, actual.Name);
    Assert.Equal(role.Description, actual.Description);
    Assert.Single(actual.Scopes);
    Assert.Equal(role.Scopes.First(), actual.Scopes.First());
    mockRoleManager.Verify(m => m.CreateAsync(It.IsAny<LgdxRole>()), Times.Once);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Once);
  }

  [Fact]
  public async Task CreateRoleAsync_CalledWithFailingCreateAsync_ShouldReturnLgdxIdentity400Expection()
  {
    // Arrange
    mockRoleManager.Setup(m => m.CreateAsync(It.IsAny<LgdxRole>())).Returns(Task.FromResult(IdentityResult.Failed()));
    var role = new LgdxRoleCreateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope1"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.CreateRoleAsync(role);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockRoleManager.Verify(m => m.CreateAsync(It.IsAny<LgdxRole>()), Times.Once);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Never);
  }

  [Fact]
  public async Task CreateRoleAsync_CalledWithFailingAddClaimAsync_ShouldReturnLgdxIdentity400Expection()
  {
    // Arrange
    mockRoleManager.Setup(m => m.CreateAsync(It.IsAny<LgdxRole>())).ReturnsAsync(IdentityResult.Success);
    mockRoleManager.Setup(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>())).Returns(Task.FromResult(IdentityResult.Failed()));
    var role = new LgdxRoleCreateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope1"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.CreateRoleAsync(role);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockRoleManager.Verify(m => m.CreateAsync(It.IsAny<LgdxRole>()), Times.Once);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Once);
  }

  [Fact]
  public async Task UpdateRoleAsync_CalledWithRoleId_ShouldReturnTrue()
  {
    // Arrange
    var id = Role1Id;
    mockRoleManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(roles.Where(r => r.Id == id.ToString()).FirstOrDefault());
    mockRoleManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxRole>())).ReturnsAsync(IdentityResult.Success);
    mockRoleManager.Setup(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
    mockRoleManager.Setup(m => m.RemoveClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
    var role = new LgdxRoleUpdateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope2"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    var actual = await roleService.UpdateRoleAsync(Role1Id, role);

    // Assert
    Assert.True(actual);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxRole>()), Times.Once);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Once);
    mockRoleManager.Verify(m => m.RemoveClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Once);
  }

  [Fact]
  public async Task UpdateRoleAsync_CalledWithSystemRole_ShouldReturnThrowsLgdxValidation400Expection()
  {
    // Arrange
    var id = Role3Id;
    var role = new LgdxRoleUpdateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope2"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.UpdateRoleAsync(id, role);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("Cannot update system role.", exception.Message);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxRole>()), Times.Never);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Never);
    mockRoleManager.Verify(m => m.RemoveClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Never);
  }

  [Fact]
  public async Task UpdateRoleAsync_CalledWithInvalidRoleId_ShouldReturnThrowsLgdxNotFound404Exception()
  {
    // Arrange
    var id = Guid.Empty;
    var role = new LgdxRoleUpdateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope2"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.UpdateRoleAsync(id, role);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxRole>()), Times.Never);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Never);
    mockRoleManager.Verify(m => m.RemoveClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Never);
  }

  [Fact]
  public async Task UpdateRoleAsync_CalledWithFailingUpdate_ShouldReturnThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var id = Role1Id;
    mockRoleManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(roles.Where(r => r.Id == id.ToString()).FirstOrDefault());
    mockRoleManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxRole>())).ReturnsAsync(IdentityResult.Failed());
    var role = new LgdxRoleUpdateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope2"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.UpdateRoleAsync(id, role);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxRole>()), Times.Once);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Never);
    mockRoleManager.Verify(m => m.RemoveClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Never);
  }

  [Fact]
  public async Task UpdateRoleAsync_CalledWithFailingAddClaim_ShouldReturnThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var id = Role1Id;
    mockRoleManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(roles.Where(r => r.Id == id.ToString()).FirstOrDefault());
    mockRoleManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxRole>())).ReturnsAsync(IdentityResult.Success);
    mockRoleManager.Setup(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed());
    var role = new LgdxRoleUpdateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope2"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.UpdateRoleAsync(id, role);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxRole>()), Times.Once);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Once);
    mockRoleManager.Verify(m => m.RemoveClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Never);
  }

  [Fact]
  public async Task UpdateRoleAsync_CalledWithFailingRemoveClaim_ShouldReturnThrowsLgdxIdentity400Expection()
  {
    // Arrange
    var id = Role1Id;
    mockRoleManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(roles.Where(r => r.Id == id.ToString()).FirstOrDefault());
    mockRoleManager.Setup(m => m.UpdateAsync(It.IsAny<LgdxRole>())).ReturnsAsync(IdentityResult.Success);
    mockRoleManager.Setup(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
    mockRoleManager.Setup(m => m.RemoveClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed());
    var role = new LgdxRoleUpdateBusinessModel {
      Name = "Test Role",
      Description = "Test Description",
      Scopes = ["scope2"]
    };
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.UpdateRoleAsync(id, role);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<LgdxRole>()), Times.Once);
    mockRoleManager.Verify(m => m.AddClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Once);
    mockRoleManager.Verify(m => m.RemoveClaimAsync(It.IsAny<LgdxRole>(), It.IsAny<Claim>()), Times.Once);
  }

  [Fact]
  public async Task DeleteRoleAsync_Called_ShouldReturnTrue()
  {
    // Arrange
    var id = Role1Id;
    mockRoleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(roles.Where(r => r.Id == id.ToString()).FirstOrDefault());
    mockRoleManager.Setup(m => m.DeleteAsync(It.IsAny<LgdxRole>())).ReturnsAsync(IdentityResult.Success);
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    var actual = await roleService.DeleteRoleAsync(id);

    // Assert
    Assert.True(actual);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockRoleManager.Verify(m => m.DeleteAsync(It.IsAny<LgdxRole>()), Times.Once);
  }

  [Fact]
  public async Task DeleteRoleAsync_CalledWithSystemRole_ShouldReturnThrowsLgdxValidation400Expection()
  {
    // Arrange
    var id = Role3Id;
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.DeleteRoleAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxValidation400Expection>(act);
    Assert.Equal("Cannot delete system role.", exception.Message);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    mockRoleManager.Verify(m => m.DeleteAsync(It.IsAny<LgdxRole>()), Times.Never);
  }

  [Fact]
  public async Task DeleteRoleAsync_CalledWithInvalidId_ShouldReturnLgdxNotFound404Exception()
  {
    // Arrange
    var id = Guid.Empty;
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.DeleteRoleAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxNotFound404Exception>(act);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockRoleManager.Verify(m => m.DeleteAsync(It.IsAny<LgdxRole>()), Times.Never);
  }

  [Fact]
  public async Task DeleteRoleAsync_CalledWithFailingDelete_ShouldReturnLgdxIdentity400Expection()
  {
    // Arrange
    var id = Role1Id;
    mockRoleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(roles.Where(r => r.Id == id.ToString()).FirstOrDefault());
    mockRoleManager.Setup(m => m.DeleteAsync(It.IsAny<LgdxRole>())).ReturnsAsync(IdentityResult.Failed());
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);

    // Act
    Task act() => roleService.DeleteRoleAsync(id);

    // Assert
    var exception = await Assert.ThrowsAsync<LgdxIdentity400Expection>(act);
    mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Once);
    mockRoleManager.Verify(m => m.DeleteAsync(It.IsAny<LgdxRole>()), Times.Once);
  }

  [Theory]
  [InlineData("")]
  [InlineData("ROLE")]
  [InlineData("ROLE 1")]
  [InlineData("AAA")]
  public async Task SearchRoleAsync_CalledWithName_ShouldReturnRolesWithName(string name)
  {
    // Arrange
    var roleService = new RoleService(lgdxContext, mockRoleManager.Object);
    var expected = roles.Where(r => r.NormalizedName!.Contains(name)).ToList();

    // Act
    var actual = await roleService.SearchRoleAsync(name);

    // Assert
    Assert.Equal(expected.Count, actual.Count());
    Assert.All(actual, r => {
      var expected = actual.FirstOrDefault(e => e.Id == r.Id);
      Assert.NotNull(expected);
      Assert.Equal(expected.Name, r.Name);
    });
  }
}