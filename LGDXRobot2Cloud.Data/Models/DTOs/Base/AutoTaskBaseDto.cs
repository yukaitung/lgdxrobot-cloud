using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Base;

public class AutoTaskBaseDto
{
  public string? Name { get; set; }
  
  public int Priority { get; set; }
  
  [Required]
  public int FlowId { get; set; }

  public Guid? AssignedRobotId { get; set; }
}
