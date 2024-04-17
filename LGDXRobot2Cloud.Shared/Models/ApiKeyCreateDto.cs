using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class ApiKeyCreateDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [MaxLength(100)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public bool IsThirdParty { get; set; }
  }
}