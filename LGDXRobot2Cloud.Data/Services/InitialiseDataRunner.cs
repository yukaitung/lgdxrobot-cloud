using System.Security.Claims;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Services;

public class InitialiseDataRunner(
    LgdxContext context,
    UserManager<LgdxUser> userManager
  ) : IHostedService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly UserManager<LgdxUser> _userManager = userManager;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    /*
     * Identity
     */
    // Roles
    var defaultRoles = LgdxRolesHelper.DefaultRoles;
    foreach (var (key, value) in defaultRoles)
    {
      var role = new LgdxRole{
        Id = key.ToString(),
        Name = value.Name,
        NormalizedName = value.Name.ToUpper(),
      };
      var roleStore = new RoleStore<LgdxRole>(context);
      if (!context.Roles.Any(r => r.Name == role.Name))
      {
        // Create Role
        await roleStore.CreateAsync(role, cancellationToken);
        // Add claims for role
        foreach (var scope in value.Scopes)
        {
          var claim = new Claim("scope", scope);
          await roleStore.AddClaimAsync(role, claim, cancellationToken);
        }
      }
    }
    // Admin User
    var firstUser = new LgdxUser
    {
      Id = Guid.CreateVersion7().ToString(),
      Email = "admin@example.com",
      EmailConfirmed = true,
      LockoutEnabled = true,
      Name = "Admin",
      NormalizedEmail = "admin@example.com".ToUpper(),
      NormalizedUserName = "ADMIN",
      SecurityStamp = Guid.NewGuid().ToString(),
      UserName = "admin"
    };

    if (!context.Users.Any(u => u.UserName == firstUser.UserName))
    {
      var password = new PasswordHasher<LgdxUser>();
      var hashed = password.HashPassword(firstUser, "123456");
      firstUser.PasswordHash = hashed;

      var userStore = new UserStore<LgdxUser>(context);
      await userStore.CreateAsync(firstUser, cancellationToken);
    }
    // Assign user to roles
    LgdxUser? user = await _userManager.FindByEmailAsync(firstUser.Email);
    var result = await _userManager.AddToRolesAsync(user!, ["Global Administrator"]);
    await context.SaveChangesAsync(cancellationToken);

    Environment.Exit(0);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}