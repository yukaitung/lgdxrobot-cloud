using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Data.Contracts;

public record AutoTaskNavProgress 
{
  public double Eta { get; set; }
  public int Recoveries { get; set; }
  public double DistanceRemaining { get; set; }
  public int WaypointsRemaining { get; set; }
}

public record RobotDof
{
  public double X { get; set; }
  public double Y { get; set; }
  public double Rotation { get; set; }
}

public record RobotCriticalStatus
{
  public bool HardwareEmergencyStop { get; set; }
  public bool SoftwareEmergencyStop { get; set; }
  public List<int> BatteryLow { get; set; } = null!;
  public List<int> MotorDamaged { get; set; } = null!;
}

public record RobotDataContract 
{
  public Guid RobotId { get; set; }
  public RobotStatus RobotStatus { get; set; }
  public RobotCriticalStatus CriticalStatus { get; set; } = null!;
  public List<double> Batteries { get; set; } = null!;
  public RobotDof Position { get; set; } = null!;
  public AutoTaskNavProgress NavProgress { get; set; } = null!;
}