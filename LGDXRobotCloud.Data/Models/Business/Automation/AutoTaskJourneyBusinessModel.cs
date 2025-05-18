using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record AutoTaskJourneyBusinessModel
{
  public required int Id { get; set; }

  public int? CurrentProcessId { get; set; }

  public string? CurrentProcessName { get; set; }

  public required DateTime CreatedTime { get; set; }
}

public static class AutoTaskJourneyBusinessModelExtensions
{
  public static AutoTaskJourneyDto ToDto(this AutoTaskJourneyBusinessModel model)
  {
    return new AutoTaskJourneyDto
    {
      Id = model.Id,
      CurrentProcessId = model.CurrentProcessId,
      CurrentProcessName = model.CurrentProcessName,
      CreatedTime = model.CreatedTime,
    };
  }
}