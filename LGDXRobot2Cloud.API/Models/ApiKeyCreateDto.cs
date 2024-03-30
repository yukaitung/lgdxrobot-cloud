using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class ApiKeyCreateDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = null!;

    [Required]
    public bool isThirdParty { get; set; }
  }
}