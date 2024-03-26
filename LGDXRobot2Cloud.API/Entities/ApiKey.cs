using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGDXRobot2Cloud.API.Entities
{
  public class ApiKey
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public bool isThirdParty { get; set; }

    [Required]
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  }
}