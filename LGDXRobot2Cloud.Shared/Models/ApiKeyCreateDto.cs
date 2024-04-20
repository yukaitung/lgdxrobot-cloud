using LGDXRobot2Cloud.Shared.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace LGDXRobot2Cloud.Shared.Models
{
  public class ApiKeyCreateDto : ApiKeyBaseDto
  {
    [MaxLength(200)]
    public string? Secret { get; set; }

    [Required]
    public bool IsThirdParty { get; set; }
  }
}