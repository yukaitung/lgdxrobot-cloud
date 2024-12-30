using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Data.Entities;

[Table("Setting.ApiKeys")]
public class ApiKey
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  [MaxLength(50)]
  [Required]
  public string Name { get; set; } = null!;

  [MaxLength(200)]
  public string Secret { get; set; } = null!;

  [Required]
  public bool IsThirdParty { get; set; }
}
