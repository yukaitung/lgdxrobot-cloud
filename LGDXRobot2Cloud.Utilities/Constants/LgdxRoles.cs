namespace LGDXRobot2Cloud.Utilities.Constants;

public record LgdxRoleData
{
  public string Name { get; set; } = null!;
  public List<string> Scopes { get; set; } = [];
}

public static class LgdxRoles
{
  public static Dictionary<Guid, LgdxRoleData> Default { get; set; } = new()
  {
    {
      new Guid("01942232-62d5-7db2-a566-f720e44ade0d"), 
      new LgdxRoleData
      {
        Name = "Global Administrator",
        Scopes = [
          "LGDXRobot2Cloud.API/FullAccess"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7b98-9fbb-137f811c2ad9"), 
      new LgdxRoleData
      {
        Name = "Global Auditor",
        Scopes = [
          "LGDXRobot2Cloud.API/Read"
        ]
      }
    },
    {
      new Guid("01942232-62d5-734a-b2fe-5caf8b22deda"), 
      new LgdxRoleData
      {
        Name = "Robot Administrator",
        Scopes = [
          "LGDXRobot2Cloud.API/Robot/Robots/FullAccess"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7dda-99de-e041a0ec179b"), 
      new LgdxRoleData
      {
        Name = "Robot Auditor",
        Scopes = [
          "LGDXRobot2Cloud.API/Robot/Robots/Read"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7a90-99c0-3716b4fd2bb1"), 
      new LgdxRoleData
      {
        Name = "Navigation Administrator",
        Scopes = [
          "LGDXRobot2Cloud.API/Navigation/FullAccess"
        ]
      }
    },
    {
      new Guid("01942232-62d5-718a-b69a-044ac485ea0c"), 
      new LgdxRoleData
      {
        Name = "Navigation Auditor",
        Scopes = [
          "LGDXRobot2Cloud.API/Navigation/Read"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7218-a36f-b37da58d0066"), 
      new LgdxRoleData
      {
        Name = "Tasks Administrator",
        Scopes = [
          "LGDXRobot2Cloud.API/Task/FullAccess"
        ]
      }
    },
    {
      new Guid("01942232-62d5-7ad9-8fe5-4ca5135d3d5f"), 
      new LgdxRoleData
      {
        Name = "Tasks Auditor",
        Scopes = [
          "LGDXRobot2Cloud.API/Tasks/Read"
        ]
      } 
    },
    {
      new Guid("01942232-62d5-755d-8cc6-dd069622cca5"), 
      new LgdxRoleData
      {
        Name = "Tasks Operator",
        Scopes = [
          "LGDXRobot2Cloud.API/Tasks/Write",
          "LGDXRobot2Cloud.API/Tasks/Delete"
        ]
      } 
    },
    {
      new Guid("01942232-62d5-7d00-b742-e41e7421bf8f"), 
      new LgdxRoleData
      {
        Name = "Notification Email Recipient",
        Scopes = []
      } 
    }
  };
}