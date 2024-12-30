using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;

public class TriggerUpdateDto
{
  [Required]
  [MaxLength(50)]
  public required string Name { get; set; }

  [Required]
  [MaxLength(200)]
  public required string Url { get; set; }

  public required int HttpMethodId { get; set; }

  public string? Body { get; set; }

  public required bool SkipOnFailure { get; set; } = false;
  
  public int? ApiKeyInsertLocationId { get; set; }

  [MaxLength(50)]
  public string? ApiKeyFieldName { get; set; }

  public int? ApiKeyId { get; set; }
}
