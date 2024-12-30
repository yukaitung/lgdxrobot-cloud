using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Data.Entities;

[Table("Automation.Flows")]
public class Flow
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(50)]
  public string Name { get; set; } = null!;

  public ICollection<FlowDetail> FlowDetails { get; set; } = [];
}
