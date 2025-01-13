using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public record ApiKeySearchBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }
}

public static class ApiKeySearchBusinessModelExtensions
{
  public static ApiKeySearchDto ToDto(this ApiKeySearchBusinessModel apiKey)
  {
    return new ApiKeySearchDto{
      Id = apiKey.Id,
      Name = apiKey.Name
    };
  }

  public static IEnumerable<ApiKeySearchDto> ToDto(this IEnumerable<ApiKeySearchBusinessModel> apiKeys)
  {
    return apiKeys.Select(a => a.ToDto());
  }
}