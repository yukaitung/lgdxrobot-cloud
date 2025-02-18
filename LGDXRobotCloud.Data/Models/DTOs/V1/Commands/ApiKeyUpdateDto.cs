using System.ComponentModel.DataAnnotations;
using LGDXRobotCloud.Data.Models.Business.Administration;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Commands;

public record ApiKeyUpdateDto 
{    
  [Required]
  [MaxLength(50)]
  public required string Name { get; set; }
}

public static class ApiKeyUpdateDtoExtensions
{
  public static ApiKeyUpdateBusinessModel ToBusinessModel(this ApiKeyUpdateDto apiKeyUpdate)
  {
    return new ApiKeyUpdateBusinessModel
    {
      Name = apiKeyUpdate.Name
    };
  }
}