using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.API.Entities
{
  public class NodesCollection
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public ICollection<Node> Nodes { get; set; } = new List<Node>();

    [Required]
    [Precision(3)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Precision(3)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  } 
}