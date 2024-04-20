using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Shared.Entities
{
  public class Progress
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = null!;

    public bool System { get; set; } = false;

    public bool Reserved { get; set; } = false;

    [Precision(3)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Precision(3)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}