using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class AutoTaskBaseDto
  {
    public string? Name { get; set; }
    
    public int Priority { get; set; }
   
    [Required]
    public int FlowId { get; set; }

    public int? AssignedRobotId { get; set; }
  }
}