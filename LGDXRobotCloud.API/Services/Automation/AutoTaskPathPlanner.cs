using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Data.DbContexts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.EntityFrameworkCore;

namespace LGDXRobotCloud.API.Services.Automation;

public interface IAutoTaskPathPlannerService
{
  Task<List<RobotClientsPath>> GeneratePath(AutoTask autoTask);
}

public sealed partial class AutoTaskPathPlannerService(
    ILogger<AutoTaskPathPlannerService> logger,
    IMapEditorService mapEditorService,
    IOnlineRobotsService onlineRobotsService,
    LgdxContext context
  ) : IAutoTaskPathPlannerService
{
  private readonly IMapEditorService _mapEditorService = mapEditorService ?? throw new ArgumentNullException(nameof(mapEditorService));
  private readonly IOnlineRobotsService _onlineRobotsService = onlineRobotsService ?? throw new ArgumentNullException(nameof(onlineRobotsService));
  private readonly LgdxContext _context = context ?? throw new ArgumentNullException(nameof(context));

  [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Path planning: The task detail does not have waypoint.")]
  public partial void TheTaskDetailDoesNotHaveWaypoint();

  [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Path planning: Unable to find the path. Start waypoint ID: {StartWaypointId}, End waypoint ID: {EndWaypointId}.")]
  public partial void TheTaskDetailDoesNotHaveWaypoint(int startWaypointId, int endWaypointId);

  [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Path planning: Robot data not found for robot ID: {RobotId}.")]
  public partial void RobotDataNotFoundForRobotId(Guid robotId);

  private static double EuclideanDistance(double x1, double y1, double x2, double y2)
  {
    return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
  }

  private static double ManhattanDistance(double x1, double y1, double x2, double y2)
  {
    return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
  }

  private static List<RobotClientsDof> PathPlanningGeneratePath(Dictionary<int, int> parentMap, int currentId, WaypointsTraffic waypointsTraffic)
  {
    List<int> path = [currentId];
    while (parentMap.TryGetValue(currentId, out int parentId))
    {
      path.Add(parentId);
      currentId = parentId;
    }
    path.Reverse();

    var pathPlanningPath = new List<RobotClientsDof>();
    foreach (var id in path)
    {
      var waypoint = waypointsTraffic.Waypoints[id]!;
      pathPlanningPath.Add(new RobotClientsDof
      {
        X = waypoint.X,
        Y = waypoint.Y,
        Rotation = waypoint.Rotation,
      });
    }
    return pathPlanningPath;
  }

  private List<RobotClientsDof> PathPlanning(AutoTaskDetail start, AutoTaskDetail end, WaypointsTraffic waypointsTraffic)
  {
    // Check waypoint
    if (start.WaypointId == null || end.WaypointId == null)
    {
      TheTaskDetailDoesNotHaveWaypoint();
      throw new Exception();
    }
    int startWaypointId = (int)start.WaypointId;
    List<int> openList = [startWaypointId];
    List<int> closedList = [];

    var gScore = new Dictionary<int, double> { [startWaypointId] = 0 };
    var hScore = new Dictionary<int, double> { [startWaypointId] = ManhattanDistance(start.Waypoint!.X, start.Waypoint!.Y, end.Waypoint!.X, end.Waypoint!.Y) };
    var parentMap = new Dictionary<int, int>();

    while (openList.Count > 0)
    {
      var currentId = openList.OrderBy(w => gScore[w] + hScore[w]).First();
      if (currentId == end.WaypointId)
      {
        return PathPlanningGeneratePath(parentMap, currentId, waypointsTraffic);
      }

      openList.Remove(currentId);
      closedList.Add(currentId);

      foreach (var neighborId in waypointsTraffic.WaypointTraffics[currentId])
      {
        var neighbor = waypointsTraffic.Waypoints[neighborId];
        if (closedList.Contains(neighbor.Id))
        {
          continue;
        }

        var current = waypointsTraffic.Waypoints[currentId];
        double newGScore = gScore[current.Id] + ManhattanDistance(current.X, current.Y, neighbor.X, neighbor.Y);
        if (!gScore.TryGetValue(neighbor.Id, out double currentGScore) || newGScore < currentGScore)
        {
          // Update gScore and hScore
          gScore[neighbor.Id] = newGScore;
          hScore[neighbor.Id] = ManhattanDistance(neighbor.X, neighbor.Y, end.Waypoint!.X, end.Waypoint!.Y);

          // Add the neighbor to the open list
          parentMap[neighbor.Id] = current.Id;
          if (!openList.Contains(neighbor.Id))
          {
            openList.Add(neighbor.Id);
          }
        }
      }
    }

    TheTaskDetailDoesNotHaveWaypoint(start.WaypointId.Value, end.WaypointId.Value);
    throw new Exception("");
  }

  private static RobotClientsDof GenerateWaypoint(AutoTaskDetail taskDetail)
  {
    if (taskDetail.Waypoint != null)
    {
      var waypoint = new RobotClientsDof
      { X = taskDetail.Waypoint.X, Y = taskDetail.Waypoint.Y, Rotation = taskDetail.Waypoint.Rotation };
      if (taskDetail.CustomX != null)
        waypoint.X = (double)taskDetail.CustomX;
      if (taskDetail.CustomY != null)
        waypoint.X = (double)taskDetail.CustomY;
      if (taskDetail.CustomRotation != null)
        waypoint.X = (double)taskDetail.CustomRotation;
      return waypoint;
    }
    else
    {
      return new RobotClientsDof
      {
        X = taskDetail.CustomX != null ? (double)taskDetail.CustomX : 0,
        Y = taskDetail.CustomY != null ? (double)taskDetail.CustomY : 0,
        Rotation = taskDetail.CustomRotation != null ? (double)taskDetail.CustomRotation : 0
      };
    }
  }

  public async Task<List<RobotClientsPath>> GeneratePath(AutoTask autoTask)
  {
    var realmId = autoTask.RealmId;
    var hasWaypointsTrafficControl = context.Realms.AsNoTracking()
      .Where(r => r.Id == realmId)
      .Select(r => r.HasWaypointsTrafficControl)
      .FirstOrDefault();

    List<RobotClientsPath> paths = [];
    List<AutoTaskDetail> taskDetails = [];
    if (autoTask.CurrentProgressId == (int)ProgressState.PreMoving)
    {
      var firstTaskDetail = await _context.AutoTasksDetail.AsNoTracking()
        .Where(t => t.AutoTaskId == autoTask.Id)
        .Include(t => t.Waypoint)
        .OrderBy(t => t.Order)
        .FirstOrDefaultAsync();
      if (firstTaskDetail != null)
        taskDetails.Add(firstTaskDetail);
    }
    else if (autoTask.CurrentProgressId == (int)ProgressState.Moving)
    {
      taskDetails = await _context.AutoTasksDetail.AsNoTracking()
        .Where(t => t.AutoTaskId == autoTask.Id)
        .Include(t => t.Waypoint)
        .OrderBy(t => t.Order)
        .ToListAsync();
    }
    if (taskDetails.Count == 0)
    {
      return [];
    }

    if (hasWaypointsTrafficControl)
    {
      // Has waypoints traffic control, return the waypoints by respecting the traffic control
      // Prepare waypoints traffic
      var waypointsTraffic = await _mapEditorService.GetWaypointTrafficAsync(realmId);

      // Find the nearest waypoint
      var robotData = _onlineRobotsService.GetRobotData((Guid)autoTask.AssignedRobotId!);
      if (robotData == null)
      {
        RobotDataNotFoundForRobotId((Guid)autoTask.AssignedRobotId!);
        throw new Exception();
      }
      int selectedWaypointId = 0;
      double selectedWaypointDistance = double.MaxValue;
      foreach (var waypoint in waypointsTraffic.Waypoints)
      {
        var distance = EuclideanDistance(waypoint.Value.X, waypoint.Value.Y, robotData.Position.X, robotData.Position.Y);
        if (distance < selectedWaypointDistance)
        {
          selectedWaypointId = waypoint.Key;
          selectedWaypointDistance = distance;
        }
      }
      taskDetails.Insert(0, new AutoTaskDetail
      {
        Id = 0,
        Order = 0,
        WaypointId = selectedWaypointId,
        Waypoint = waypointsTraffic.Waypoints[selectedWaypointId],
        AutoTaskId = autoTask.Id,
        AutoTask = autoTask
      });

      // Path planning using A*
      for (int i = 1; i < taskDetails.Count; i++)
      {
        var start = taskDetails[i - 1];
        var end = taskDetails[i];
        var planningPath = PathPlanning(start, end, waypointsTraffic);

        // Use custom waypoint for end
        var planningPathEnd = planningPath.Last();
        if (end.CustomX != null)
        {
          planningPathEnd.X = (double)end.CustomX;
        }
        if (end.CustomY != null)
        {
          planningPathEnd.Y = (double)end.CustomY;
        }
        if (end.CustomRotation != null)
        {
          planningPathEnd.Rotation = (double)end.CustomRotation;
        }
        planningPath.RemoveAt(planningPath.Count - 1);
        planningPath.Add(planningPathEnd);

        paths.Add(new RobotClientsPath
        {
          Waypoints = { planningPath }
        });
      }
    }
    else
    {
      // No waypoints traffic control, just return the waypoints
      foreach (var t in taskDetails)
      {
        paths.Add(new RobotClientsPath
        {
          Waypoints = { GenerateWaypoint(t) }
        });
      }
    }
    return paths;
  }
}