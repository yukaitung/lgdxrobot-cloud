using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Base;

public class NodesCollectionBaseDto
{
  [Required]
  public string Name { get; set; } = null!;
}
