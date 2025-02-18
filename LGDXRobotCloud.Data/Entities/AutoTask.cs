using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.Data.Entities;

[Table("Automation.AutoTasks")]
[Index(nameof(RealmId), nameof(AssignedRobotId))]
public class AutoTask
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(50)]
  public string? Name { get; set; }
  
  public ICollection<AutoTaskDetail> AutoTaskDetails { get; set; } = [];

  public int Priority { get; set; }

  [ForeignKey("FlowId")]
  public Flow? Flow { get; set; } = null!;

  public int? FlowId { get; set; }

  [ForeignKey("RealmId")]
  public Realm Realm { get; set; } = null!;

  public int RealmId { get; set; }

  [ForeignKey("AssignedRobotId")]
  public Robot? AssignedRobot { get; set; }

  public Guid? AssignedRobotId { get; set; }

  [ForeignKey("CurrentProgressId")]
  public Progress CurrentProgress { get; set; } = null!;

  public int CurrentProgressId { get; set; }

  public int? CurrentProgressOrder { get; set; }

  [MaxLength(32)]
  public string? NextToken { get; set; }
}
