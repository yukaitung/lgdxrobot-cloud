using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.API.Models
{
  public class TriggerCreateDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Url { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
    public string ApiKeyLocationStr { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ApiKeyName { get; set; } = string.Empty;
    public int ApiKeyId { get; set; }
  }
}