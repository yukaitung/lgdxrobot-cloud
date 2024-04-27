using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.Shared.Entities
{
  [Table("Constant.SystemComponents")]
  public class SystemComponent
  {
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = null!;
  }
}