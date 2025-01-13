using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public record ApiKeyBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }
  
  public required bool IsThirdParty { get; set; }
}

public static class ApiKeyBusinessModelExtensions
{
  public static ApiKeyDto ToDto(this ApiKeyBusinessModel apiKey)
  {
    return new ApiKeyDto{
      Id = apiKey.Id,
      Name = apiKey.Name,
      IsThirdParty = apiKey.IsThirdParty
    };
  }

  public static IEnumerable<ApiKeyDto> ToDto(this IEnumerable<ApiKeyBusinessModel> apiKeys)
  {
    return apiKeys.Select(a => a.ToDto());
  }
}