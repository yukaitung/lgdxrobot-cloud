using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class Flow
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public ICollection<Progress> Progresses { get; set; } = new List<Progress>();

    [Required]
    [ForeignKey("SignalEmitterId")]
    public SignalEmitter ProceedCondition { get; set; } = new SignalEmitter();

    public int SignalEmitterId { get; set; }
  
    [Required]
    public ICollection<Trigger> StartTriggers { get; set; } = new List<Trigger>();

    [Required]
    public ICollection<Trigger> EndTriggers { get; set; } = new List<Trigger>();

    [Required]
    public DateTime CreateAt { get; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}