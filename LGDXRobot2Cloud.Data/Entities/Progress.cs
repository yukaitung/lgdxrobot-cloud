using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Data.Entities;

[Table("Automation.Progresses")]
public class Progress
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(50)]
  public string Name { get; set; } = null!;

  public bool System { get; set; } = false;

  public bool Reserved { get; set; } = false;
}
