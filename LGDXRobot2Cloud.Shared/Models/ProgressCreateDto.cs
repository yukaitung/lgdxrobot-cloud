using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class ProgressCreateDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
  }
}