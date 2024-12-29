using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobot2Cloud.Data.Entities;

[Table("Navigation.Certificates")]
public class RobotCertificate
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public Guid Id { get; set; }

  [MaxLength(40)]
  public string Thumbprint { get; set; } = null!;

  [MaxLength(40)]
  public string? ThumbprintBackup { get; set; }

  [Precision(0)]
  public DateTime NotBefore { get; set; }

  [Precision(0)]
  public DateTime NotAfter { get; set; }

  [ForeignKey("RobotId")]
  public Robot Robot { get; set; } = null!;

  public Guid RobotId { get; set; }
}