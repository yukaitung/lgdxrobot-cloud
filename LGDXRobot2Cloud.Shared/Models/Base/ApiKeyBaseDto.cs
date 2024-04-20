using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class ApiKeyBaseDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
  }
}