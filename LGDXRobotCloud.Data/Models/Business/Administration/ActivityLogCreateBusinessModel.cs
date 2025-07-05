using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Models.Business.Administration;

public class ActivityLogCreateBusinessModel
{
  public required string EntityName { get; set; }

  public required string EntityId { get; set; }

  public required ActivityAction Action { get; set; }

  public string? Note { get; set; }
}