using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class Progress
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public bool System { get; set; }

    [Required]
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Flow> Flows { get; set; } = new List<Flow>();
  }
}