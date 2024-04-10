using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class TriggerCreateDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Url { get; set; } = null!;

    public string? Body { get; set; }
    
    [Required]
    public string ApiKeyLocationStr { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string ApiKeyName { get; set; } = null!;

    [Required]
    public int ApiKeyId { get; set; }
  }
}