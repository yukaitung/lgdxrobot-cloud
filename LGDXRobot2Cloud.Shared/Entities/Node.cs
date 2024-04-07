using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Shared.Entities
{
  public class Node
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(50)]
    public required string ProcessName { get; set; }

    [MaxLength(200)]
    public string? Arguments { get; set; }

    [Precision(3)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Precision(3)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}