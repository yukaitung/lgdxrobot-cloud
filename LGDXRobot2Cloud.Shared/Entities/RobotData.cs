namespace LGDXRobot2Cloud.Shared.Entities
{
  // Not part of Database
  public record RobotData
  {
    public (double, double, double, double) Batteries;
    public (double, double, double) Position;
    public (bool, bool) EmergencyStopsEnabled;
    public double Eta;
    public int Recoveries;
    public double DistanceRemaining;
    public int WaypointsRemaining;
  }
}