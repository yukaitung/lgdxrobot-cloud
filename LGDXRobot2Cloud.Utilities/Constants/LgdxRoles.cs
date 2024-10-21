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
      new Guid("07b16cb2-5daf-4d0b-a67f-13f80eff2833"), 
      new LgdxRoleData
      {
        Name = "Global Administrator",
        Scopes = [
          "LGDXRobot2Cloud.API/FullAccess"
        ]
      }
    },
    {
      new Guid("0fb45ce9-d492-4cbc-8e58-24de5deb193d"), 
      new LgdxRoleData
      {
        Name = "Global Auditor",
        Scopes = [
          "LGDXRobot2Cloud.API/Read"
        ]
      }
    },
    {
      new Guid("62779402-22d2-4c66-83d4-5d1aab7b2834"), 
      new LgdxRoleData
      {
        Name = "Robot Administrator",
        Scopes = [
          "LGDXRobot2Cloud.API/Robot/Robots/FullAccess"
        ]
      }
    },
    {
      new Guid("f14119aa-00de-4404-94c1-89440104be7e"), 
      new LgdxRoleData
      {
        Name = "Robot Auditor",
        Scopes = [
          "LGDXRobot2Cloud.API/Robot/Robots/Read"
        ]
      }
    },
    {
      new Guid("ca7fb8ed-d7b0-423f-b241-5740e1fd6475"), 
      new LgdxRoleData
      {
        Name = "Navigation Administrator",
        Scopes = [
          "LGDXRobot2Cloud.API/Navigation/FullAccess"
        ]
      }
    },
    {
      new Guid("69525f08-e48f-4c52-8ab7-3ed41ac269af"), 
      new LgdxRoleData
      {
        Name = "Navigation Auditor",
        Scopes = [
          "LGDXRobot2Cloud.API/Navigation/Read"
        ]
      }
    },
    {
      new Guid("b5ffffa8-238e-47e9-8db1-99b7c7591a1d"), 
      new LgdxRoleData
      {
        Name = "Tasks Administrator",
        Scopes = [
          "LGDXRobot2Cloud.API/Task/FullAccess"
        ]
      }
    },
    {
      new Guid("3abb5eea-d7b5-4756-98a1-7f5dc4e98af9"), 
      new LgdxRoleData
      {
        Name = "Tasks Auditor",
        Scopes = [
          "LGDXRobot2Cloud.API/Tasks/Read"
        ]
      } 
    },
    {
      new Guid("8493293c-74d2-48e8-ce83-0671007c5d7a"), 
      new LgdxRoleData
      {
        Name = "Tasks Operator",
        Scopes = [
          "LGDXRobot2Cloud.API/Tasks/Write",
          "LGDXRobot2Cloud.API/Tasks/Delete"
        ]
      } 
    }
  };
}