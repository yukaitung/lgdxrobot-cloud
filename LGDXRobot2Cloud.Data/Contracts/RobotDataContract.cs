namespace LGDXRobot2Cloud.Data.Contracts;

public record RobotDof
{
  public double X { get; set; }
  public double Y { get; set; }
  public double Rotation { get; set; }
}

public record RobotDataContract 
{
  public Guid RobotId { get; set; }
  public RobotDof Position { get; set; } = null!;
}