using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public class AutoTaskUpdateDto
{
  public string? Name { get; set; }

  public required IEnumerable<AutoTaskDetailUpdateDto> AutoTaskDetails { get; set; } = [];
  
  [Required]
  public required int Priority { get; set; }
  
  [Required]
  public required int FlowId { get; set; }

  [Required]
  public required int RealmId { get; set; }

  public Guid? AssignedRobotId { get; set; }
}
