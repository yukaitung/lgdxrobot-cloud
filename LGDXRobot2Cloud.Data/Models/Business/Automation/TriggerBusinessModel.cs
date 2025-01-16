using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobot2Cloud.Data.Models.Business.Automation;

public record TriggerBusinessModel
{
  public required int Id { get; set; }

  public required string Name { get; set; }

  public required string Url { get; set; }

  public required int HttpMethodId { get; set; }

  public string? Body { get; set; }

  public required bool SkipOnFailure { get; set; }
  
  public int? ApiKeyInsertLocationId { get; set; }

  public string? ApiKeyFieldName { get; set; }

  public int? ApiKeyId { get; set; }

  public string? ApiKeyName { get; set; }
}

public static class TriggerBusinessModelExtensions
{
  public static TriggerDto ToDto(this TriggerBusinessModel triggerBusinessModel)
  {
    return new TriggerDto {
      Id = triggerBusinessModel.Id,
      Name = triggerBusinessModel.Name,
      Url = triggerBusinessModel.Url,
      HttpMethodId = triggerBusinessModel.HttpMethodId,
      Body = triggerBusinessModel.Body,
      SkipOnFailure = triggerBusinessModel.SkipOnFailure,
      ApiKeyInsertLocationId = triggerBusinessModel.ApiKeyInsertLocationId,
      ApiKeyFieldName = triggerBusinessModel.ApiKeyFieldName,
      ApiKey = triggerBusinessModel.ApiKeyId != null ? new ApiKeySearchDto {
        Id = triggerBusinessModel.ApiKeyId.Value,
        Name = triggerBusinessModel.ApiKeyName!,
      } : null,
    };
  }
}