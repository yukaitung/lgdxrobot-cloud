using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public class ActivityLogListDto
{
  public int Id { get; set; }

  public string EntityName { get; set; } = default!;

  public string EntityId { get; set; } = default!;

  public ActivityAction Action { get; set; }

  public LgdxUserSearchDto? User { get; set; }

  public DateTime CreatedAt { get; set; }
}