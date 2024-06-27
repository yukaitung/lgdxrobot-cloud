namespace LGDXRobot2Cloud.Shared.Entities
{
  // Not part of Database
  public class RobotData
  {
    public (double, double, double, double) Batteries;
    public (double, double, double) Position;
    public (double, double, double) Velocity;
    public (bool, bool) EmergencyStopsEnabled;
  }
}