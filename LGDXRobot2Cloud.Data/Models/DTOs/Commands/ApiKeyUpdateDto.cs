using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Commands;

public class ApiKeyUpdateDto 
{    
  
  [Required]
  [MaxLength(50)]
  public string Name { get; set; } = null!;
}
