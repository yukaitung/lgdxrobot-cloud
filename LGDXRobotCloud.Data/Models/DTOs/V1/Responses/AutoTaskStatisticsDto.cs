namespace LGDXRobotCloud.Data.Models.DTOs.V1.Responses;

public record AutoTaskStatisticsDto
{
  public required int WaitingTasks { get; set; }

  public required int RunningTasks { get; set; }
  
  public required int CompletedTasks { get; set; }

  public required int AbortedTasks { get; set; }

  public required IEnumerable<int> WaitingTasksTrend { get; set; }

  public required IEnumerable<int> RunningTasksTrend { get; set; }

  public required IEnumerable<int> CompletedTasksTrend { get; set; }

  public required IEnumerable<int> AbortedTasksTrend { get; set; }

  public required DateTime LastUpdatedAt { get; set; }
}