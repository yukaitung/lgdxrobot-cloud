namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class RobotCreateDto
{
  public RobotCreateInfoDto RobotInfo { get; set; } = null!;
  public RobotCreateChassisInfo RobotChassisInfo { get; set; } = null!;
}
