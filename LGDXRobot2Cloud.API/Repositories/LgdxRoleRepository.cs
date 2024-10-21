using LGDXRobot2Cloud.Data.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface ILgdxRoleRepository
{
 Task<IEnumerable<IdentityRole>> GetRolesAsync(IList<string> roles);
 Task<IEnumerable<IdentityRoleClaim<string>>> GetRolesClaimsAsync(List<string> roleIds);
}

public class LgdxRoleRepository(LgdxContext context) : ILgdxRoleRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  public async Task<IEnumerable<IdentityRole>> GetRolesAsync(IList<string> roles)
  {
    return await _context.Roles
      .Where(r => roles.Contains(r.Name!))
      .ToListAsync();
  }

  public async Task<IEnumerable<IdentityRoleClaim<string>>> GetRolesClaimsAsync(List<string> roleIds)
  {
    return await _context.RoleClaims
      .Where(r => roleIds.Contains(r.RoleId))
      .ToListAsync();
  }
}