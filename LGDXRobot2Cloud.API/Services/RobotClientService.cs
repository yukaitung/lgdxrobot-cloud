using Grpc.Core;
using LGDXRobot2Cloud.Protos;
using static LGDXRobot2Cloud.Protos.RobotClientService;

namespace LGDXRobot2Cloud.Services
{
  public class RobotClientService : RobotClientServiceBase
  {
    public override Task<TaskProgressDetail> Exchange(RobotData data, ServerCallContext context)
    {
      Console.WriteLine("Task<TaskProgressDetail> Exchange(RobotData data, ServerCallContext context)");
      return Task.FromResult(new TaskProgressDetail());
    }

    public override Task<StatusMessage> CompleteTaskProgress(CompleteToken token, ServerCallContext context)
    {
      Console.WriteLine("Task<StatusMessage> CompleteTaskProgress(CompleteToken token, ServerCallContext context)");
      return Task.FromResult(new StatusMessage());
    }

    public override Task<StatusMessage> UpdateRobotSpecification(RobotSpecification specification, ServerCallContext context)
    {
      Console.WriteLine("Task<StatusMessage> UpdateRobotSpecification(RobotSpecification specification, ServerCallContext context)");
      return Task.FromResult(new StatusMessage());
    }
  }
}