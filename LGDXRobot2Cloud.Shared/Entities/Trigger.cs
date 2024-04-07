using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Shared.Entities
{
  public class Trigger
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public required string Url { get; set; }

    public string? Body { get; set; }

    [ForeignKey("ApiKeyLocationId")]
    public ApiKeyLocation? ApiKeyLocation { get; set; }

    public int ApiKeyLocationId;

    [MaxLength(50)]
    public string? ApiKeyName { get; set; }

    [ForeignKey("ApiKeyId")]
    public ApiKey? ApiKey { get; set; }

    public int ApiKeyId { get; set; }
    
    [Precision(3)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Precision(3)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}