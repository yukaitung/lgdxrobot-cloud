using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Models.RabbitMQ;

public record ActivityLogRequest
{
  public required string EntityName { get; set; }

  public required string EntityId { get; set; }

  public required ActivityAction Action { get; set; }

  public Guid? UserId { get; set; }

  public int? ApiKeyId { get; set; }

  public string? Note { get; set; }
}