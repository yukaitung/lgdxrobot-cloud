using LGDXRobot2Cloud.Data.Entities;

namespace LGDXRobot2Cloud.Data.Models.Business.Administration;

public record ApiKeyCreateBusinessModel
{
  public required string Name { get; set; }
  
  public string? Secret { get; set; }

  public required bool IsThirdParty { get; set; }
}

public static class ApiKeyCreateBusinessModelExtensions
{
  public static ApiKey ToEntity(this ApiKeyCreateBusinessModel apiKey)
  {
    return new ApiKey{
      Name = apiKey.Name,
      Secret = apiKey.Secret!,
      IsThirdParty = apiKey.IsThirdParty
    };
  }
}