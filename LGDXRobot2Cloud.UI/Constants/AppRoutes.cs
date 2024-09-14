namespace LGDXRobot2Cloud.UI.Constants;

public static class AppRoutes
{
  public const string Home = "/";

  public static class Navigation
  {
    public static class Map
    {
      public const string Index = "/navigation/map";
    }
    public static class Tasks
    {
      public const string Index = "/navigation/tasks";
    }
    public static class Waypoints
    {
      public const string Index = "/navigation/waypoints";
    }
    public static class Flows
    {
      public const string Index = "/navigation/flows";
    }
    public static class Triggers
    {
      public const string Index = "/navigation/triggers";
    }
    public static class Progresses
    {
      public const string Index = "/navigation/progresses";
    }
  }

  public static class Robot 
  {
    public static class Robots
    {
      public const string Index = "/robot/robots";
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
    }
  }

  public static class Setting
  {
    public static class ApiKeys
    {
      public const string Index = "/setting/apikeys";
    }
  }
}