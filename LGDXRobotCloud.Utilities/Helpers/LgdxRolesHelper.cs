using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Utilities.Helpers;

public record LgdxRoleData
{
  public required LgdxRoleType RoleType { get; set; }
  public required string Name { get; set; }
  public List<string> Scopes { get; set; } = [];
}

public class LgdxRolesHelper
{
  public static Dictionary<Guid, LgdxRoleData> DefaultRoles { get; set; } = new()
  {
    {
      new Guid("01942232-62d5-7db2-a566-f720e44ade0d"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.GlobalAdministrator,
        Name = LgdxRoleType.GlobalAdministrator.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/FullAccess"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7b98-9fbb-137f811c2ad9"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.GlobalAuditor,
        Name = LgdxRoleType.GlobalAuditor.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Read"
        ]
      }
    },
    {
      new Guid("01942232-62d5-734a-b2fe-5caf8b22deda"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.RobotAdministrator,
        Name = LgdxRoleType.RobotAdministrator.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Navigation/Robots/FullAccess"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7dda-99de-e041a0ec179b"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.RobotAuditor,
        Name = LgdxRoleType.RobotAuditor.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Navigation/Robots/Read"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7a90-99c0-3716b4fd2bb1"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.NavigationAdministrator,
        Name = LgdxRoleType.NavigationAdministrator.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Navigation/FullAccess"
        ]
      }
    },
    {
      new Guid("01942232-62d5-718a-b69a-044ac485ea0c"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.NavigationAuditor,
        Name = LgdxRoleType.NavigationAuditor.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Navigation/Read"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7218-a36f-b37da58d0066"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.AutomationAdministrator,
        Name = LgdxRoleType.AutomationAdministrator.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Automation/FullAccess"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7ad9-8fe5-4ca5135d3d5f"),
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.AutomationAuditor,
        Name = LgdxRoleType.AutomationAuditor.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Automation/Read"
        ]
      } 
    },
    {
      new Guid("01942232-62d5-755d-8cc6-dd069622cca5"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.AutoTaskAdministrator,
        Name = LgdxRoleType.AutoTaskAdministrator.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Automation/AutoTasks/FullAccess"
        ]
      } 
    },
    {
      new Guid("01942232-62d5-7d00-b742-e41e7421bf8f"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.AutoTaskAuditor,
        Name = LgdxRoleType.AutoTaskAuditor.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Automation/AutoTasks/Read"
        ]
      } 
    },
    {
      new Guid("01942323-e76a-79ef-9502-8f3894194070"),
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.AutoTaskOperator,
        Name = LgdxRoleType.AutoTaskOperator.ToEnumMember()!,
        Scopes = [
          "LGDXRobotCloud.API/Automation/AutoTasks/Write",
          "LGDXRobotCloud.API/Automation/AutoTasks/Delete"
        ]
      }
    },
    {
      new Guid("01942323-e76a-7ce8-a4f9-d550527ffe4e"), 
      new LgdxRoleData
      {
        RoleType = LgdxRoleType.EmailRecipient,
        Name = LgdxRoleType.EmailRecipient.ToEnumMember()!,
        Scopes = []
      }
    }
  };

  public static bool IsSystemRole(Guid roleId)
  {
    return DefaultRoles.ContainsKey(roleId);
  }

  public static Guid GetSystemRoleId(LgdxRoleType roleType)
  {
    return DefaultRoles.First(r => r.Value.RoleType == roleType).Key;
  }
}