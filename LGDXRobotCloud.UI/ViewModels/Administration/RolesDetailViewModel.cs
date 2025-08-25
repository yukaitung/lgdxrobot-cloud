using System.ComponentModel.DataAnnotations;
using System.Text;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Administration;

public static class ScopeOptionsSelect
{
  public static readonly List<string> Areas = ["Select... / All Area", "Administration", "Automation", "Navigation"];
  public static readonly List<List<string>> Controllers = [
    [],
    ["ApiKeys", "RobotCertificates", "Roles", "Users"],
    ["AutoTasks", "Flows", "Progresses", "Triggers", "TriggerRetries"],
    ["Realms", "Waypoints", "Robots"]
  ];
  public static readonly List<string> Permissions = ["FullAccess", "Read", "Write", "Delete"];
}

public record ScopeOption
{
  public int Area { get; set; } = 0;
  public int? Controller { get; set; } = null;
  public int? Permission { get; set; } = null;
}

public class RolesDetailViewModel : FormViewModel, IValidatableObject
{
  public Guid Id { get; set; }
  
  [Required (ErrorMessage = "Please enter a name.")]
  public string Name { get; set; } = null!;

  public string? Description { get; set; }

  public List<ScopeOption> Scopes { get; set; } = [];

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    int i = 0;
    foreach (var scope in Scopes)
    {
      if (scope.Permission == null)
      {
        yield return new ValidationResult($"Permission is required for the scope.", [$"{nameof(ScopeOption.Permission)}-{i}"]);
      }
      i++;
    }
  }
}

public static class RolesDetailViewModelExtensions
{
  private static List<ScopeOption> DtoToScopes(List<string> scopes)
  {
    List<ScopeOption> scopeOptions = [];
    foreach (var scope in scopes)
    {
      string[] split = scope.Split("/");
      ScopeOption scopeOption = new();
      if (split.Length == 2)
      {
        int permission = ScopeOptionsSelect.Permissions.IndexOf(split[1]);
        scopeOption.Permission = permission == -1 ? null : permission;
      }
      else if (split.Length == 3)
      {
        int area = ScopeOptionsSelect.Areas.IndexOf(split[1]);
        scopeOption.Area = area == -1 ? 0 : area;
        int permission = ScopeOptionsSelect.Permissions.IndexOf(split[2]);
        scopeOption.Permission = permission == -1 ? null : permission;
      }
      else if (split.Length == 4)
      {
        int area = ScopeOptionsSelect.Areas.IndexOf(split[1]);
        scopeOption.Area = area == -1 ? 0 : area;
        int controller = ScopeOptionsSelect.Controllers[scopeOption.Area].IndexOf(split[2]);
        scopeOption.Controller = controller == -1 ? null : controller;
        int permission = ScopeOptionsSelect.Permissions.IndexOf(split[3]);
        scopeOption.Permission = permission == -1 ? null : permission;
      }
      scopeOptions.Add(scopeOption);
    }
    return scopeOptions;
  }

  private static List<string> ScopesToDto(List<ScopeOption> scopes)
  {
    List<string> stringList = [];
    foreach (var scope in scopes)
    {
      StringBuilder sb = new("LGDXRobotCloud.API");
      if (scope.Area != 0)
      {
        sb.Append($"/{ScopeOptionsSelect.Areas[scope.Area]}");
      }
      if (scope.Controller != null)
      {
        sb.Append($"/{ScopeOptionsSelect.Controllers[scope.Area][(int)scope.Controller]}");
      }
      if (scope.Permission != null)
      {
        sb.Append($"/{ScopeOptionsSelect.Permissions[(int)scope.Permission]}");
      }
      stringList.Add(sb.ToString());
    }
    return stringList;
  }

  public static void FromDto(this RolesDetailViewModel roleDetailViewModel, LgdxRoleDto lgdxRoleDto)
  {
    roleDetailViewModel.Id = (Guid)lgdxRoleDto.Id!;
    roleDetailViewModel.Name = lgdxRoleDto.Name!;
    roleDetailViewModel.Description = lgdxRoleDto.Description;
    roleDetailViewModel.Scopes = DtoToScopes(lgdxRoleDto.Scopes!);
  }

  public static LgdxRoleUpdateDto ToUpdateDto(this RolesDetailViewModel roleDetailViewModel)
  {
    return new LgdxRoleUpdateDto {
      Name = roleDetailViewModel.Name,
      Description = roleDetailViewModel.Description,
      Scopes = ScopesToDto(roleDetailViewModel.Scopes)
    };
  }

  public static LgdxRoleCreateDto ToCreateDto(this RolesDetailViewModel roleDetailViewModel)
  {
    return new LgdxRoleCreateDto {
      Name = roleDetailViewModel.Name,
      Description = roleDetailViewModel.Description,
      Scopes = ScopesToDto(roleDetailViewModel.Scopes)
    };
  }
}