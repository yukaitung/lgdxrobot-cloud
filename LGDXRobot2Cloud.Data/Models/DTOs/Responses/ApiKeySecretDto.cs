using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Data.Models.DTOs.Responses;

public class ApiKeySecretDto
{
  [MaxLength(200)]
  public string Secret { get; set; } = null!;
}
