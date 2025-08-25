using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Models.Redis;

public record AutoTaskNavProgress
{
  public double Eta { get; set; }
  public int Recoveries { get; set; }
  public double DistanceRemaining { get; set; }
  public int WaypointsRemaining { get; set; }
  public List<Robot2Dof> Plan { get; set; } = [];
}

public record Robot2Dof
{
  public double X { get; set; }
  public double Y { get; set; }
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
  public List<int> BatteryLow { get; set; } = [];
  public List<int> MotorDamaged { get; set; } = [];
}

public record RobotData
{
  public RobotStatus RobotStatus { get; set; } = RobotStatus.Offline;
  public RobotCriticalStatus CriticalStatus { get; set; } = new();
  public List<double> Batteries { get; set; } = [];
  public RobotDof Position { get; set; } = new();
  public AutoTaskNavProgress NavProgress { get; set; } = new();
  public bool PauseTaskAssignment { get; set; }
}