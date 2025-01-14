using System.Security.Claims;
using LGDXRobot2Cloud.API.Exceptions;
using LGDXRobot2Cloud.Data.DbContexts;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Data.Models.Business.Administration;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Services.Administration;

public interface IRoleService
{
  Task<(IEnumerable<LgdxRoleListBusinessModel>, PaginationHelper)> GetRolesAsync(string? name, int pageNumber, int pageSize);
  Task<LgdxRoleBusinessModel> GetRoleAsync(Guid id);
  Task<LgdxRoleBusinessModel> CreateRoleAsync(LgdxRoleCreateBusinessModel lgdxRoleBusinessModel);
  Task<bool> UpdateRoleAsync(Guid id, LgdxRoleUpdateBusinessModel lgdxRoleUpdateBusinessModel);
  Task<bool> DeleteRoleAsync(Guid id);

  Task<IEnumerable<LgdxRoleSearchBusinessModel>> SearchRoleAsync(string? name);
}

public class RoleService(
    LgdxContext context,
    RoleManager<LgdxRole> roleManager
  ) : IRoleService
{
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));
  private readonly RoleManager<LgdxRole> _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));

  public async Task<(IEnumerable<LgdxRoleListBusinessModel>, PaginationHelper)> GetRolesAsync(string? name, int pageNumber, int pageSize)
  {
    var query = _context.Roles as IQueryable<LgdxRole>;
    if (!string.IsNullOrWhiteSpace(name))
    {
      name = name.Trim().ToUpper();
      query = query.Where(u => u.NormalizedName!.Contains(name));
    }
    var itemCount = await query.CountAsync();
    var PaginationHelper = new PaginationHelper(itemCount, pageNumber, pageSize);
    var roles = await query.AsNoTracking()
      .OrderBy(t => t.Id)
      .Skip(pageSize * (pageNumber - 1))
      .Take(pageSize)
      .Select(t => new LgdxRoleListBusinessModel {
        Id = Guid.Parse(t.Id!),
        Name = t.Name!,
        Description = t.Description,
      })
      .ToListAsync();
    return (roles, PaginationHelper);
  }

  public async Task<LgdxRoleBusinessModel> GetRoleAsync(Guid id)
  {
    var role = await _context.Roles.AsNoTracking()
      .Where(r => r.Id == id.ToString())
      .Select(r => new{
        r.Id,
        r.Name,
        r.Description,
      })
      .FirstOrDefaultAsync() 
        ?? throw new LgdxNotFound404Exception();
    
    var claims = await _context.RoleClaims.AsNoTracking()
      .Where(c => c.RoleId == id.ToString())
      .Where(c => c.ClaimType == "scope")
      .Select(r => new {
        ClaimValue = r.ClaimValue!,
      }).ToListAsync();

    return new LgdxRoleBusinessModel {
      Id = Guid.Parse(role.Id),
      Name = role.Name!,
      Description = role.Description,
      Scopes = claims.Select(c => c.ClaimValue!),
    };
  }

  public async Task<LgdxRoleBusinessModel> CreateRoleAsync(LgdxRoleCreateBusinessModel lgdxRoleBusinessModel)
  {
    var role = new LgdxRole {
      Name = lgdxRoleBusinessModel.Name,
      Description = lgdxRoleBusinessModel.Description,
      NormalizedName = lgdxRoleBusinessModel.Name.ToUpper(),
    };
    var result = await _roleManager.CreateAsync(role);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }

    if (lgdxRoleBusinessModel.Scopes.Any())
    {
      foreach (var scope in lgdxRoleBusinessModel.Scopes)
      {
        var addScopeResult = await _roleManager.AddClaimAsync(role, new Claim("scope", scope));
        if (!addScopeResult.Succeeded)
        {
          throw new LgdxIdentity400Expection(addScopeResult.Errors);
        }
      }      
    }

    return new LgdxRoleBusinessModel {
      Id = Guid.Parse(role.Id),
      Name = role.Name!,
      Description = role.Description,
      Scopes = lgdxRoleBusinessModel.Scopes,
    };
  }

  public async Task<bool> UpdateRoleAsync(Guid id, LgdxRoleUpdateBusinessModel lgdxRoleUpdateBusinessModel)
  {
    if (LgdxRolesHelper.IsSystemRole(id))
    {
      throw new LgdxValidation400Expection(nameof(lgdxRoleUpdateBusinessModel.Name), "Cannot update system role.");
    }
    var role = await _roleManager.FindByIdAsync(id.ToString()) ?? throw new LgdxNotFound404Exception();

    role.Name = lgdxRoleUpdateBusinessModel.Name;
    role.Description = lgdxRoleUpdateBusinessModel.Description;
    role.NormalizedName = lgdxRoleUpdateBusinessModel.Name.ToUpper();

    var result = await _roleManager.UpdateAsync(role);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }

    var scopes = await _context.RoleClaims
      .Where(r => r.RoleId == id.ToString())
      .Where(r => r.ClaimType == "scope")
      .Select(r => new {
        r.ClaimValue
      })
      .ToListAsync();
    var scopesList = scopes.Select(s => s.ClaimValue).ToList();
    var scopeToAdd = lgdxRoleUpdateBusinessModel.Scopes.Except(scopesList);
    foreach (var scope in scopeToAdd)
    {
      var addScopeResult = await _roleManager.AddClaimAsync(role, new Claim("scope", scope!));
      if (!addScopeResult.Succeeded)
      {
        throw new LgdxIdentity400Expection(addScopeResult.Errors);
      }
    }
    var scopeToRemove = scopesList.Except(lgdxRoleUpdateBusinessModel.Scopes);
    foreach (var scope in scopeToRemove)
    {
      var removeScopeResult = await _roleManager.RemoveClaimAsync(role, new Claim("scope", scope!));
      if (!removeScopeResult.Succeeded)
      {
        throw new LgdxIdentity400Expection(removeScopeResult.Errors);
      }
    }
    return true; // Error is handled in the Identity service
  }

  public async Task<bool> DeleteRoleAsync(Guid id)
  {
    if (LgdxRolesHelper.IsSystemRole(id))
    {
      throw new LgdxValidation400Expection(nameof(id), "Cannot delete system role.");
    }
    var role = await _roleManager.FindByIdAsync(id.ToString()) ?? throw new LgdxNotFound404Exception();
    var result = await _roleManager.DeleteAsync(role);
    if (!result.Succeeded)
    {
      throw new LgdxIdentity400Expection(result.Errors);
    }
    return true; // Error is handled in the Identity service
  }

  public async Task<IEnumerable<LgdxRoleSearchBusinessModel>> SearchRoleAsync(string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return await _context.Roles.AsNoTracking()
        .Take(10)
        .Select(t => new LgdxRoleSearchBusinessModel {
          Id = Guid.Parse(t.Id!),
          Name = t.Name!,
        })
        .ToListAsync();
    }
    else
    {
      return await _context.Roles.AsNoTracking()
        .Where(w => w.Name!.Contains(name))
        .Take(10)
        .Select(t => new LgdxRoleSearchBusinessModel {
          Id = Guid.Parse(t.Id!),
          Name = t.Name!,
        })
        .ToListAsync();
    }
  }
}