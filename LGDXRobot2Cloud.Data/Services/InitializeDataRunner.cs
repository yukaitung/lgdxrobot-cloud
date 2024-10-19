using LGDXRobot2Cloud.Data.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Services;

public class InitializeDataRunner(LgdxContext context,
  UserManager<IdentityUser> userManager) : IHostedService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly UserManager<IdentityUser> _userManager = userManager;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    /*
     * Identity
     */
    // Roles
    List<IdentityRole> roles = [
      new IdentityRole { Id = "07b16cb2-5daf-4d0b-a67f-13f80eff2833", Name = "Global Administrator", NormalizedName = "Global Administrator".ToUpper() },
      new IdentityRole { Id = "0fb45ce9-d492-4cbc-8e58-24de5deb193d", Name = "Global Reader", NormalizedName = "Global Reader".ToUpper() },
      new IdentityRole { Id = "62779402-22d2-4c66-83d4-5d1aab7b2834", Name = "Robot Administrator", NormalizedName = "Robot Administrator".ToUpper() },
      new IdentityRole { Id = "f14119aa-00de-4404-94c1-89440104be7e", Name = "Robot Reader", NormalizedName = "Robot Reader".ToUpper() },
      new IdentityRole { Id = "ca7fb8ed-d7b0-423f-b241-5740e1fd6475", Name = "Navigation Administrator", NormalizedName = "Navigation Administrator".ToUpper() },
      new IdentityRole { Id = "69525f08-e48f-4c52-8ab7-3ed41ac269af", Name = "Navigation Reader", NormalizedName = "Navigation Reader".ToUpper() },
      new IdentityRole { Id = "b5ffffa8-238e-47e9-8db1-99b7c7591a1d", Name = "Task Administrator", NormalizedName = "Task Administrator".ToUpper() },
      new IdentityRole { Id = "3abb5eea-d7b5-4756-98a1-7f5dc4e98af9", Name = "Task Reader", NormalizedName = "Task Reader".ToUpper() }
    ];
    foreach (var role in roles)
    {
      var roleStore = new RoleStore<IdentityRole>(context);
      if (!context.Roles.Any(r => r.Name == role.Name))
      {
        await roleStore.CreateAsync(role, cancellationToken);
      }
    }
    // Admin User
    var firstUser = new IdentityUser
    {
      Email = "admin@example.com",
      NormalizedEmail = "admin@example.com".ToUpper(),
      UserName = "admin",
      NormalizedUserName = "ADMIN",
      EmailConfirmed = true,
      SecurityStamp = Guid.NewGuid().ToString("D")
    };

    if (!context.Users.Any(u => u.UserName == firstUser.UserName))
    {
      var password = new PasswordHasher<IdentityUser>();
      var hashed = password.HashPassword(firstUser,"123456");
      firstUser.PasswordHash = hashed;

      var userStore = new UserStore<IdentityUser>(context);
      await userStore.CreateAsync(firstUser, cancellationToken);
    }
    // Assign user to roles
    IdentityUser? user = await _userManager.FindByEmailAsync(firstUser.Email);
    var result = await _userManager.AddToRolesAsync(user!, ["Global Administrator"]);
    await context.SaveChangesAsync(cancellationToken);

    /*
     * SQL Script
     */
    var path = Path.Combine(Directory.GetCurrentDirectory(), "SQL", "MySQL.sql");
    string sql = await File.ReadAllTextAsync(path, cancellationToken);
    sql = sql.Replace("delimiter //", string.Empty);
    sql = sql.Replace("delimiter ;", string.Empty);
    sql = sql.Replace("//", ";");
    await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);

    Environment.Exit(0);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}