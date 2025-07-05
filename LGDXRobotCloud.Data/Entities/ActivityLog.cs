using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Entities;

[Index(nameof(EntityName))]
[Index(nameof(EntityName), nameof(EntityId))]
public class ActivityLog
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  public required string EntityName { get; set; }

  public required string EntityId { get; set; }

  public required int Action { get; set; }

  public Guid? UserId { get; set; }

  public int? ApiKeyId { get; set; }

  public string? Note { get; set; }

  public DateTime CreatedAt { get; set; }
}