using LGDXRobotCloud.API.Services.Administration;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Wolverine;

namespace LGDXRobotCloud.API.UnitTests.Services.Navigation;

public class OnlineRobotsServiceTests 
{
  private static readonly int RealmId = 1;

  private readonly Mock<IActivityLogService> mockActivityLogService = new();
  private readonly Mock<IMessageBus> mockBus = new();
  private readonly Mock<IEmailService> mockEmailService = new();
  private readonly Mock<IMemoryCache> mockMemoryCache = new();
  private readonly Mock<IRobotService> mockRobotService = new();

  public OnlineRobotsServiceTests()
  {
    mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);
    mockRobotService.Setup(m => m.GetRobotRealmIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult((int?)RealmId));
  }
}