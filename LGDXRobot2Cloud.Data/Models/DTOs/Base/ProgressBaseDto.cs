using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Base;

public class ProgressBaseDto
{
  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = null!;
}
