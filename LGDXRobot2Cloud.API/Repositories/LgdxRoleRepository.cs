using System.Security.Claims;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Repositories;

public interface ILgdxRoleRepository
{
  Task<(IEnumerable<LgdxRole>, PaginationHelper)> GetRolesAsync(string? name, int pageNumber, int pageSize);
  Task<IEnumerable<LgdxRole>> GetRolesAsync(IList<string> roles);
  Task<IEnumerable<IdentityRoleClaim<string>>> GetRolesClaimsAsync(List<string> roleIds);
  Task<IEnumerable<IdentityRoleClaim<string>>> GetRoleScopesAsync(string roleId);
  Task<bool> AddRoleScopesAsync(LgdxRole role, IEnumerable<string> scopes);
  Task<bool> RemoveRoleScopesAsync(LgdxRole role, IEnumerable<string> scopes);
  Task<IEnumerable<LgdxRole>> SearchRolesAsync(string name);
}

public class LgdxRoleRepository(LgdxContext context) : ILgdxRoleRepository
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  public async Task<(IEnumerable<LgdxRole>, PaginationHelper)> GetRolesAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Roles as IQueryable<LgdxRole>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim().ToUpper();
      query = query.Where(u => u.NormalizedName!.Contains(name));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var roles = await query
      .OrderBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .ToListAsync();
    return (roles, PaginationHelper);
  }

  public async Task<IEnumerable<LgdxRole>> GetRolesAsync(IList<string> roles)
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

  public async Task<IEnumerable<IdentityRoleClaim<string>>> GetRoleScopesAsync(string roleId)
  {
    return await _context.RoleClaims
      .Where(r => r.RoleId == roleId)
      .Where(r => r.ClaimType == "scope")
      .ToListAsync();
  }

  public async Task<bool> AddRoleScopesAsync(LgdxRole role, IEnumerable<string> scopes)
  {
    var roleStore = new RoleStore<LgdxRole>(_context);
    foreach (var scope in scopes)
    {
      var claim = new Claim("scope", scope);
      await roleStore.AddClaimAsync(role, claim);
    }
    return await _context.SaveChangesAsync() > 0;
  }

  public async Task<bool> RemoveRoleScopesAsync(LgdxRole role, IEnumerable<string> scopes)
  {
    var roleStore = new RoleStore<LgdxRole>(_context);
    foreach (var scope in scopes)
    {
      var claim = new Claim("scope", scope);
      await roleStore.RemoveClaimAsync(role, claim);
    }
    return await _context.SaveChangesAsync() > 0;
  }

  public async Task<IEnumerable<LgdxRole>> SearchRolesAsync(string name)
  {
    return await _context.Roles.AsNoTracking()
      .Where(r => r.NormalizedName!.Contains(name))
      .Take(10)
      .ToListAsync();
  }
}