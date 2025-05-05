using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Entities;

[Table("Automation.Triggers")]
[Index(nameof(ApiKeyInsertLocationId))]
public class Trigger
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(50)]
  [Required]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  [Required]
  public string Url { get; set; } = null!;

  public int HttpMethodId { get; set; }

  public string? Body { get; set; }

  public int? ApiKeyInsertLocationId { get; set; }

  [MaxLength(50)]
  public string? ApiKeyFieldName { get; set; } // Header name or Json name

  [ForeignKey("ApiKeyId")]
  public ApiKey? ApiKey { get; set; }

  public int? ApiKeyId { get; set; }
}
