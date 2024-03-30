using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Entities
{
  public class Trigger
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Url { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    [ForeignKey("ApiKeyLocationId")]
    public ApiKeyLocation? ApiKeyLocation { get; set; }

    public int ApiKeyLocationId;

    [MaxLength(50)]
    public string? ApiKeyName { get; set; }

    [ForeignKey("ApiKeyId")]
    public ApiKey? ApiKey { get; set; }

    public int ApiKeyId { get; set; }
    
    [Required]
    [Precision(3)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Precision(3)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}