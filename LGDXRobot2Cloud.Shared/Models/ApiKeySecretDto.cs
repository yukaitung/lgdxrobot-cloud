using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class ApiKeySecretDto
  {
    [MaxLength(100)]
    public string Key { get; set; } = null!;
  }
}