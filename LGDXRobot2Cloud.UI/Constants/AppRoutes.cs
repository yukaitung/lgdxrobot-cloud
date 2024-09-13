namespace LGDXRobot2Cloud.UI.Constants;

public static class AppRoutes
{
  public const string Home = "/";

  public static class Robot 
  {
    public static class Nodes
    {
      public const string Index = "/robot/nodes";
      public const string Create = "/robot/nodes/create";
      public const string Detail = "/robot/nodes/{Id:int}";
    }
  }
}