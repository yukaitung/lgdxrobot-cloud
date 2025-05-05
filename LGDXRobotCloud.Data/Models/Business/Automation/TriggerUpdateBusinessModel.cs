namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record TriggerUpdateBusinessModel
{
  public required string Name { get; set; }

  public required string Url { get; set; }

  public required int HttpMethodId { get; set; }

  public string? Body { get; set; }
  
  public int? ApiKeyInsertLocationId { get; set; }

  public string? ApiKeyFieldName { get; set; }

  public int? ApiKeyId { get; set; }
}