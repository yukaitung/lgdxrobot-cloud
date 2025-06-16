namespace LGDXRobotCloud.UI.Constants;

public static class AppRoutes
{
  public const string Home = "/";
  
  public static class Identity
  {
    public const string Login = "/Login";
    public const string Logout = "/Logout";
    public const string ForgotPassword = "/ForgotPassword";
    public const string ResetPassword = "/ResetPassword";
    public const string User = "/User";
  }

  public static class Navigation
  {
    public static class Realms
    {
      public const string Index = "/Navigation/Realms";
      public const string Create = "/Navigation/Realms/Create";
      public const string Detail = "/Navigation/Realms/{Id:int}";
    }
    public static class Robots
    {
      public const string Index = "/Navigation/Robots";
      public const string Create = "/Navigation/Robots/Create";
      public const string Detail = "/Navigation/Robots/{Id}";
    }
    public static class Waypoints
    {
      public const string Index = "/Navigation/Waypoints";
      public const string Create = "/Navigation/Waypoints/Create";
      public const string Detail = "/Navigation/Waypoints/{Id:int}";
    }
    public static class MapEditor
    {
      public const string Index = "/Navigation/MapEditor";
    }
  }

  public static class Automation
  {
    public static class AutoTasks
    {
      public const string Index = "/Automation/Tasks";
      public const string Create = "/Automation/Tasks/Create";
      public const string Detail = "/Automation/Tasks/{Id:int}";
    }
    public static class Flows
    {
      public const string Index = "/Automation/Flows";
      public const string Create = "/Automation/Flows/Create";
      public const string Detail = "/Automation/Flows/{Id:int}";
    }
    public static class Progresses
    {
      public const string Index = "/Automation/Progresses";
      public const string Create = "/Automation/Progresses/Create";
      public const string Detail = "/Automation/Progresses/{Id:int}";
    }
    public static class Triggers
    {
      public const string Index = "/Automation/Triggers";
      public const string Create = "/Automation/Triggers/Create";
      public const string Detail = "/Automation/Triggers/{Id:int}";
    }
    public static class TriggerRetries
    {
      public const string Index = "/Automation/TriggerRetries";
      public const string Detail = "/Automation/TriggerRetries/{Id:int}";
    }
  }

  public static class Administration
  {
    public static class ApiKeys
    {
      public const string Index = "/Administration/ApiKeys";
      public const string Create = "/Administration/ApiKeys/Create";
      public const string Detail = "/Administration/ApiKeys/{Id:int}";
      public const string SecretDetail = "/Administration/ApiKeys/{Id:int}/Secret";
    }

    public static class RobotCertificates
    {
      public const string Index = "/Administration/RobotCertificates";
      public const string Detail = "/Administration/RobotCertificates/{Id}";
      public const string Renew = "/Administration/RobotCertificates/{Id}/Renew";
    }

    public static class Users
    {
      public const string Index = "/Administration/Users";
      public const string Create = "/Administration/Users/Create";
      public const string Detail = "/Administration/Users/{Id}";
    }

    public static class Roles
    {
      public const string Index = "/Administration/Roles";
      public const string Create = "/Administration/Roles/Create";
      public const string Detail = "/Administration/Roles/{Id}";
    }
  }
}