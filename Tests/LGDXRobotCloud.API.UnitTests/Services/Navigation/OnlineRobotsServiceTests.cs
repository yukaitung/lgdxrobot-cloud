using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.API.UnitTests.Utilities;
using LGDXRobotCloud.Data.Contracts;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Protos;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Services.Navigation;

public class OnlineRobotsServiceTests 
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");
  private static readonly int RealmId = 1;
  private static string GetOnlineRobotsKey(int realmId) => $"OnlineRobotsService_OnlineRobots_{realmId}";
  private static string GetRobotCommandsKey(Guid robotId) => $"OnlineRobotsService_RobotCommands_{robotId}";

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly Mock<IBus> mockBus = new();
  private readonly Mock<IEmailService> mockEmailService = new();
  private readonly Mock<IMemoryCache> mockMemoryCache = new();
  private readonly Mock<IRobotService> mockRobotService = new();

  public OnlineRobotsServiceTests()
  {
    mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);
    mockRobotService.Setup(m => m.GetRobotRealmIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult((int?)RealmId));
  }


}