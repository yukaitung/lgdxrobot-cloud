namespace LGDXRobot2Cloud.UI.Constants;

public static class AppRoutes
{
  public const string Home = "/";

  public static class Navigation
  {
    public static class Maps
    {
      public const string Index = "/navigation/maps";
      public const string Create = "/navigation/maps/create";
      public const string Detail = "/navigation/maps/{Id:int}";
    }
    public static class Tasks
    {
      public const string Index = "/navigation/tasks";
      public const string Create = "/navigation/tasks/create";
      public const string Detail = "/navigation/tasks/{Id:int}";
    }
    public static class Waypoints
    {
      public const string Index = "/navigation/waypoints";
      public const string Create = "/navigation/waypoints/create";
      public const string Detail = "/navigation/waypoints/{Id:int}";
    }
    public static class Flows
    {
      public const string Index = "/navigation/flows";
      public const string Create = "/navigation/flows/create";
      public const string Detail = "/navigation/flows/{Id:int}";
    }
    public static class Triggers
    {
      public const string Index = "/navigation/triggers";
      public const string Create = "/navigation/triggers/create";
      public const string Detail = "/navigation/triggers/{Id:int}";
    }
    public static class Progresses
    {
      public const string Index = "/navigation/progresses";
      public const string Create = "/navigation/progresses/create";
      public const string Detail = "/navigation/progresses/{Id:int}";
    }
  }

  public static class Robot 
  {
    public static class Robots
    {
      public const string Index = "/robot/robots";
      public const string Create = "/robot/robots/create";
      public const string Detail = "/robot/robots/{Id}";
    }
    public static class Nodes
    {
      public const string Index = "/robot/nodes";
      public const string Create = "/robot/nodes/create";
      public const string Detail = "/robot/nodes/{Id:int}";
    }
    public static class NodesCollections
    {
      public const string Index = "/robot/collections";
      public const string Create = "/robot/collections/create";
      public const string Detail = "/robot/collections/{Id:int}";
    }
  }

  public static class Setting
  {
    public static class ApiKeys
    {
      public const string Index = "/setting/apikeys";
      public const string Create = "/setting/apikeys/create";
      public const string Detail = "/setting/apikeys/{Id:int}";
      public const string SecretDetail = "/setting/apikeys/{Id:int}/secret";
    }

    public static class Certificates
    {
      public const string Index = "/setting/certificates";
      public const string Detail = "/setting/certificates/{Id}";
      public const string Renew = "/setting/certificates/{Id}/renew";
    }

    public static class Users
    {
      public const string Index = "/setting/users";
      public const string Create = "/setting/users/create";
      public const string Detail = "/setting/users/{Id}";
    }

    public static class Roles
    {
      public const string Index = "/setting/roles";
      public const string Create = "/setting/roles/create";
      public const string Detail = "/setting/roles/{Id}";
    }
  }
}