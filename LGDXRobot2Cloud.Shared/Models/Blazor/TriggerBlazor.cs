using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models.Blazor
{
  public class TriggerBlazor
  {
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Url { get; set; } = null!;
    public string? Body { get; set; }
    public string? ApiKeyLocation { get; set; }

    [Required]
    [MaxLength(50)]
    public string? ApiKeyName { get; set; }
    public ApiKeyDto? ApiKey { get; set; }

    [Required]
    public int? ApiKeyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
  }
}