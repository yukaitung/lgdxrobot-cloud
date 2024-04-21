using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Base
{
  public class TriggerBaseDto
  {
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Url { get; set; } = null!;

    public string? Body { get; set; }
    
    public string? ApiKeyInsertAt { get; set; }

    [MaxLength(50)]
    public string? ApiKeyFieldName { get; set; }

    public int? ApiKeyId { get; set; }
  }
}