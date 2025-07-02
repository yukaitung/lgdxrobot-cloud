namespace LGDXRobotCloud.Data.Models.Business.Administration;

public class ActivityLogCreateBusinessModel
{
  public required string EntityName { get; set; }

  public required string EntityId { get; set; }

  public required int Action { get; set; }

  public Guid? UserId { get; set; }

  public int? ApiKeyId { get; set; }

  public string? Note { get; set; }
}