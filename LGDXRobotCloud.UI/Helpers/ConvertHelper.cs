using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.UI.Client.Models;

namespace LGDXRobotCloud.UI.Helpers;

public static class ConvertHelper
{
  public static AutoTaskListDto ToAutoTaskListDto(AutoTaskUpdate autoTaskUpdate, string RealmName)
  {
    return new AutoTaskListDto{
      Id = autoTaskUpdate.Id,
      Name = autoTaskUpdate.Name,
      Priority = autoTaskUpdate.Priority,
      Flow = new FlowSearchDto {
        Id = autoTaskUpdate.FlowId,
        Name = autoTaskUpdate.FlowName
      },
      Realm = new RealmSearchDto {
        Id = autoTaskUpdate.RealmId,
        Name = RealmName
      },
      AssignedRobot = new RobotSearchDto2 {
        Id = autoTaskUpdate.AssignedRobotId,
        Name = autoTaskUpdate.AssignedRobotName
      },
      CurrentProgress = new ProgressSearchDto {
        Id = autoTaskUpdate.CurrentProgressId,
        Name = autoTaskUpdate.CurrentProgressName
      }
    };
  }
}