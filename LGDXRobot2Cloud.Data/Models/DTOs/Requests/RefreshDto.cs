using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Requests;

public class RefreshDto
{
  [Required]
  public string RefreshToken { get; set; } = null!;
}