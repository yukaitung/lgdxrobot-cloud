using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobotCloud.Data.Entities;

[Table("Navigation.Robots")]
public class Robot
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }

  [MaxLength(50)]
  [Required]
  public string Name { get; set; } = null!;

  [ForeignKey("RealmId")]
  public Realm Realm { get; set; } = null!;

  [Required]
  public int RealmId { get; set; }

  public bool IsProtectingHardwareSerialNumber { get; set; }

  public RobotCertificate RobotCertificate { get; set; } = null!;

  public RobotSystemInfo? RobotSystemInfo { get; set; }

  public RobotChassisInfo? RobotChassisInfo { get; set; }

  public ICollection<AutoTask> AssignedTasks { get; set; } = [];
}
