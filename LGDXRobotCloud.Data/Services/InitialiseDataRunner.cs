using System.Security.Claims;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Services;

public class InitialiseDataRunner(
    LgdxContext context,
    LgdxLogsContext logsContext,
    UserManager<LgdxUser> userManager,
    IConfiguration configuration
  ) : IHostedService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly LgdxLogsContext _logsContext = logsContext ?? throw new ArgumentNullException(nameof(logsContext));
  private readonly UserManager<LgdxUser> _userManager = userManager;
  private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _context.Database.Migrate();
    _logsContext.Database.Migrate();
    if (_context.Users.Any())
    {
      return;
    }
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
      var roleStore = new RoleStore<LgdxRole>(_context);
      if (!_context.Roles.Any(r => r.Name == role.Name))
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
      Email = _configuration["email"],
      EmailConfirmed = true,
      LockoutEnabled = true,
      Name = _configuration["fullName"],
      NormalizedEmail = _configuration["email"]!.ToUpper(),
      NormalizedUserName = _configuration["userName"]!.ToUpper(),
      SecurityStamp = Guid.NewGuid().ToString(),
      UserName = _configuration["userName"]
    };

    if (!_context.Users.Any(u => u.UserName == firstUser.UserName))
    {
      var password = new PasswordHasher<LgdxUser>();
      var hashed = password.HashPassword(firstUser, _configuration["password"]!);
      firstUser.PasswordHash = hashed;

      var userStore = new UserStore<LgdxUser>(_context);
      await userStore.CreateAsync(firstUser, cancellationToken);
    }
    // Assign user to roles
    LgdxUser? user = await _userManager.FindByEmailAsync(firstUser.Email!);
    var result = await _userManager.AddToRolesAsync(user!, ["Global Administrator"]);
    await _context.SaveChangesAsync(cancellationToken);

    // Seed Data
    var isSeedData = _configuration["seedData"];
    if (!string.IsNullOrEmpty(isSeedData) && bool.Parse(isSeedData) == true)
    {
      var seeder = new DataSeeder(_context);
      await seeder.Seed();
    }

    Environment.Exit(0);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}