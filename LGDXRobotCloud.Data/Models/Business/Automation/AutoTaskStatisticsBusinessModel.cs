using LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

namespace LGDXRobotCloud.Data.Models.Business.Automation;

public record AutoTaskStatisticsBusinessModel
{
  public int WaitingTasks { get; set; } = 0;
  public int RunningTasks { get; set; } = 0;
  public int CompletedTasks { get; set; } = 0;
  public int AbortedTasks { get; set; } = 0;
  public List<int> WaitingTasksTrend { get; set; } = [];
  public List<int> RunningTasksTrend { get; set; } = [];
  public List<int> CompletedTasksTrend { get; set; } = [];
  public List<int> AbortedTasksTrend { get; set; } = [];
  public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
}

public static class AutoTaskStatisticsBusinessModelExtensions
{
  public static AutoTaskStatisticsDto ToDto(this AutoTaskStatisticsBusinessModel model)
  {
    return new AutoTaskStatisticsDto
    {
      WaitingTasks = model.WaitingTasks,
      RunningTasks = model.RunningTasks,
      CompletedTasks = model.CompletedTasks,
      AbortedTasks = model.AbortedTasks,
      WaitingTasksTrend = model.WaitingTasksTrend,
      RunningTasksTrend = model.RunningTasksTrend,
      CompletedTasksTrend = model.CompletedTasksTrend,
      AbortedTasksTrend = model.AbortedTasksTrend,
      LastUpdatedAt = model.LastUpdatedAt,
    };
  }
}