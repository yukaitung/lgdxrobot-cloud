using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class ApiKeySecretDto
  {
    [MaxLength(200)]
    public string Secret { get; set; } = null!;
  }
}