using LGDXRobotCloud.API.Configurations;
using LGDXRobotCloud.API.Services.Automation;
using LGDXRobotCloud.API.Services.Common;
using LGDXRobotCloud.API.Services.Navigation;
using LGDXRobotCloud.Protos;
using Grpc.Core.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using RobotClientsService = LGDXRobotCloud.API.Services.RobotClientsService;
using Grpc.Core;
using System.Security.Claims;
using LGDXRobotCloud.Data.Models.Business.Navigation;
using LGDXRobotCloud.Data.Models.Business.Administration;
using LGDXRobotCloud.Data.Entities;
using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.API.UnitTests.Services;

public class RobotClientsServiceTests
{
  private static readonly Guid RobotGuid = Guid.Parse("8b609e85-5865-472b-8ced-6c936ee5f127");

  private readonly RobotBusinessModel robot = new() {
    Id = RobotGuid,
    Name = "Robot",
    RealmId = 1,
    RealmName = "Realm",
    IsRealtimeExchange = true,
    IsProtectingHardwareSerialNumber = true,
    RobotCertificate = new RobotCertificateBusinessModel {
      Id = RobotGuid,
      RobotId = RobotGuid,
      RobotName = "Robot",
      Thumbprint = "Thumbprint",
      ThumbprintBackup = "ThumbprintBackup",
      NotBefore = DateTime.Now,
      NotAfter = DateTime.Now
    },
    AssignedTasks = []
  };

  private readonly RobotBusinessModel robotWithSystemInfo = new() {
    Id = RobotGuid,
    Name = "Robot",
    RealmId = 1,
    RealmName = "Realm",
    IsRealtimeExchange = true,
    IsProtectingHardwareSerialNumber = true,
    RobotCertificate = new RobotCertificateBusinessModel {
      Id = RobotGuid,
      RobotId = RobotGuid,
      RobotName = "Robot",
      Thumbprint = "Thumbprint",
      ThumbprintBackup = "ThumbprintBackup",
      NotBefore = DateTime.Now,
      NotAfter = DateTime.Now
    },
    RobotSystemInfo = new RobotSystemInfoBusinessModel {
      Id = 1,
      Cpu = "Cpu",
      IsLittleEndian = true,
      Motherboard = "Motherboard",
      MotherboardSerialNumber = "MotherboardSerialNumber",
      RamMiB = 1,
      Gpu = "Gpu",
      Os = "Os",
      Is32Bit = true,
      McuSerialNumber = "McuSerialNumber",
    },
    AssignedTasks = []
  };

  private readonly RobotClientsAutoTask robotClientsAutoTask = new() {
    TaskId = 1,
    TaskName = "Task",
    TaskProgressId = 1,
    TaskProgressName = "TaskProgress",
    NextToken = "NextToken"
  };

  private readonly Mock<IAutoTaskSchedulerService> mockAutoTaskSchedulerService = new();
  private readonly Mock<IEventService> mockEventService = new();
  private readonly Mock<IOnlineRobotsService> mockOnlineRobotsService = new();
  private readonly Mock<IRobotService> mockRobotService = new();
  private readonly Mock<IOptionsSnapshot<LgdxRobotCloudSecretConfiguration>> mockConfiguration;
  private readonly LgdxRobotCloudSecretConfiguration lgdxRobotCloudSecretConfiguration = new();

  public RobotClientsServiceTests()
  {
    lgdxRobotCloudSecretConfiguration.RobotClientsJwtSecret = "12345678901234567890123456789012";
    mockConfiguration = new Mock<IOptionsSnapshot<LgdxRobotCloudSecretConfiguration>>();
    mockConfiguration.Setup(o => o.Value).Returns(lgdxRobotCloudSecretConfiguration);
    mockOnlineRobotsService.Setup(m => m.GetRobotCommands(RobotGuid)).Returns(new RobotClientsRobotCommands {
      AbortTask = true,
      RenewCertificate = true,
      SoftwareEmergencyStop = true,
      PauseTaskAssigement = true,
    });
  }

  private static ServerCallContext GenerateServerCallContext(string functionName, string? robotId)
  {
    var httpContext = new DefaultHttpContext();
    if (robotId != null)
      httpContext.User.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, robotId)]));
    var serverCallContext = TestServerCallContext.Create(
      functionName,
      "127.0.0.1",
      DateTime.UtcNow.AddMinutes(1),
      [],
      CancellationToken.None,
      "127.0.0.1",
      null,
      null,
      (metadata) => Task.CompletedTask,
      () => new WriteOptions(),
      (writeOptions) => {}
    );
    serverCallContext.UserState["__HttpContext"] = httpContext;
    return serverCallContext;
  }

  private static readonly RobotClientsGreet robotClientsGreet = new() {
    SystemInfo = new RobotClientsSystemInfo {
      Cpu = "Cpu",
      IsLittleEndian = true,
      Motherboard = "Motherboard",
      MotherboardSerialNumber = "MotherboardSerialNumber",
      RamMiB = 1,
      Gpu = "Gpu",
      Os = "Os",
      Is32Bit = true,
      McuSerialNumber = "McuSerialNumber",
    }
  };

  private static readonly RobotChassisInfoBusinessModel robotClientsChassisInfo = new() {
    RobotTypeId = 1,
    ChassisLengthX = 1,
    ChassisLengthY = 2,
    ChassisWheelCount = 3,
    ChassisWheelRadius = 4,
    BatteryCount = 5,
    BatteryMaxVoltage = 6,
    BatteryMinVoltage = 7,
  };

  [Fact]
  public async Task Greet_Called_ShouldReturnRobotClientsGreetRespond()
  {
    // Arrange
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Greet), RobotGuid.ToString());
    mockRobotService.Setup(m => m.GetRobotAsync(RobotGuid)).ReturnsAsync(robot);
    mockRobotService.Setup(m => m.GetRobotChassisInfoAsync(RobotGuid)).ReturnsAsync(robotClientsChassisInfo);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Greet(robotClientsGreet, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Success, actural.Status);
    Assert.NotEmpty(actural.AccessToken);
    Assert.True(actural.IsRealtimeExchange);
    mockRobotService.Verify(m => m.GetRobotAsync(RobotGuid), Times.Once);
    mockRobotService.Verify(m => m.CreateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoCreateBusinessModel>()), Times.Once);
    mockRobotService.Verify(m => m.UpdateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoUpdateBusinessModel>()), Times.Never);
    mockOnlineRobotsService.Verify(m => m.AddRobotAsync(RobotGuid), Times.Once);
  }

  [Fact]
  public async Task Greet_CalledWithInvalidRobotId_ShouldReturnRobotClientsGreetErrorRespond()
  {
    // Arrange
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Greet), "InvalidRobotId");
    mockRobotService.Setup(m => m.GetRobotAsync(RobotGuid)).ReturnsAsync(robot);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Greet(robotClientsGreet, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Failed, actural.Status);
    Assert.Empty(actural.AccessToken);
    mockRobotService.Verify(m => m.GetRobotAsync(RobotGuid), Times.Never);
    mockRobotService.Verify(m => m.CreateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoCreateBusinessModel>()), Times.Never);
    mockRobotService.Verify(m => m.UpdateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoUpdateBusinessModel>()), Times.Never);
    mockOnlineRobotsService.Verify(m => m.AddRobotAsync(RobotGuid), Times.Never);
  }

  [Fact]
  public async Task Greet_CalledWitMissingRobotId_ShouldReturnRobotClientsGreetErrorRespond()
  {
    // Arrange
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Greet), null);
    mockRobotService.Setup(m => m.GetRobotAsync(RobotGuid)).ReturnsAsync(robot);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Greet(robotClientsGreet, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Failed, actural.Status);
    Assert.Empty(actural.AccessToken);
    mockRobotService.Verify(m => m.GetRobotAsync(RobotGuid), Times.Never);
    mockRobotService.Verify(m => m.CreateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoCreateBusinessModel>()), Times.Never);
    mockRobotService.Verify(m => m.UpdateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoUpdateBusinessModel>()), Times.Never);
    mockOnlineRobotsService.Verify(m => m.AddRobotAsync(RobotGuid), Times.Never);
  }

  [Fact]
  public async Task Greet_CalledWitInexistRobotId_ShouldReturnRobotClientsGreetErrorRespond()
  {
    // Arrange
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Greet), RobotGuid.ToString());
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Greet(robotClientsGreet, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Failed, actural.Status);
    Assert.Empty(actural.AccessToken);
    mockRobotService.Verify(m => m.GetRobotAsync(RobotGuid), Times.Once);
    mockRobotService.Verify(m => m.CreateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoCreateBusinessModel>()), Times.Never);
    mockRobotService.Verify(m => m.UpdateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoUpdateBusinessModel>()), Times.Never);
    mockOnlineRobotsService.Verify(m => m.AddRobotAsync(RobotGuid), Times.Never);
  }

  [Fact]
  public async Task Greet_CalledWithRobotSystemInfo_ShouldReturnRobotClientsGreetRespond()
  {
    // Arrange
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Greet), RobotGuid.ToString());
    mockRobotService.Setup(m => m.GetRobotAsync(RobotGuid)).ReturnsAsync(robotWithSystemInfo);
    mockRobotService.Setup(m => m.GetRobotChassisInfoAsync(RobotGuid)).ReturnsAsync(robotClientsChassisInfo);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Greet(robotClientsGreet, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Success, actural.Status);
    Assert.NotEmpty(actural.AccessToken);
    Assert.True(actural.IsRealtimeExchange);
    mockRobotService.Verify(m => m.GetRobotAsync(RobotGuid), Times.Once);
    mockRobotService.Verify(m => m.CreateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoCreateBusinessModel>()), Times.Never);
    mockRobotService.Verify(m => m.UpdateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoUpdateBusinessModel>()), Times.Once);
    mockOnlineRobotsService.Verify(m => m.AddRobotAsync(RobotGuid), Times.Once);
  }

  [Fact]
  public async Task Greet_CalledWithRobotSystemInfoDisparencyMotherboardSerialNumber_ShouldReturnRobotClientsGreetRespond()
  {
    // Arrange
    var localRobotClientsGreet = new RobotClientsGreet {
      SystemInfo = new RobotClientsSystemInfo {
        Cpu = "Cpu",
        IsLittleEndian = true,
        Motherboard = "Motherboard",
        MotherboardSerialNumber = "123123",
        RamMiB = 1,
        Gpu = "Gpu",
        Os = "Os",
        Is32Bit = true,
        McuSerialNumber = "McuSerialNumber",
      }
    };
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Greet), RobotGuid.ToString());
    mockRobotService.Setup(m => m.GetRobotAsync(RobotGuid)).ReturnsAsync(robotWithSystemInfo);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Greet(localRobotClientsGreet, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Failed, actural.Status);
    Assert.Empty(actural.AccessToken);
    mockRobotService.Verify(m => m.GetRobotAsync(RobotGuid), Times.Once);
    mockRobotService.Verify(m => m.CreateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoCreateBusinessModel>()), Times.Never);
    mockRobotService.Verify(m => m.UpdateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoUpdateBusinessModel>()), Times.Never);
    mockOnlineRobotsService.Verify(m => m.AddRobotAsync(RobotGuid), Times.Never);
  }

  [Fact]
  public async Task Greet_CalledWithRobotSystemInfoDisparencyMcuSerialNumber_ShouldReturnRobotClientsGreetRespond()
  {
    // Arrange
    var localRobotClientsGreet = new RobotClientsGreet {
      SystemInfo = new RobotClientsSystemInfo {
        Cpu = "Cpu",
        IsLittleEndian = true,
        Motherboard = "Motherboard",
        MotherboardSerialNumber = "MotherboardSerialNumber",
        RamMiB = 1,
        Gpu = "Gpu",
        Os = "Os",
        Is32Bit = true,
        McuSerialNumber = "123123",
      }
    };
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Greet), RobotGuid.ToString());
    mockRobotService.Setup(m => m.GetRobotAsync(RobotGuid)).ReturnsAsync(robotWithSystemInfo);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Greet(localRobotClientsGreet, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Failed, actural.Status);
    Assert.Empty(actural.AccessToken);
    mockRobotService.Verify(m => m.GetRobotAsync(RobotGuid), Times.Once);
    mockRobotService.Verify(m => m.CreateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoCreateBusinessModel>()), Times.Never);
    mockRobotService.Verify(m => m.UpdateRobotSystemInfoAsync(RobotGuid, It.IsAny<RobotSystemInfoUpdateBusinessModel>()), Times.Never);
    mockOnlineRobotsService.Verify(m => m.AddRobotAsync(RobotGuid), Times.Never);
  }

  private static readonly RobotClientsExchange robotClientsExchange = new() {
    RobotStatus = RobotClientsRobotStatus.Idle,
    CriticalStatus = new RobotClientsRobotCriticalStatus {
      HardwareEmergencyStop = true,
      SoftwareEmergencyStop = true,
    },
    Position = new RobotClientsDof {
      X = 1,
      Y = 2,
      Rotation = 3,
    },
    NavProgress = new RobotClientsAutoTaskNavProgress {
      Eta = 1,
      Recoveries = 2,
      DistanceRemaining = 3,
      WaypointsRemaining = 4,
    }
  };

  [Fact]
  public async Task Exchange_Called_ShouldReturnRobotClientsRespond()
  {
    // Arrange
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Exchange), RobotGuid.ToString());
    mockAutoTaskSchedulerService.Setup(m => m.GetAutoTaskAsync(RobotGuid)).ReturnsAsync(robotClientsAutoTask);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Exchange(robotClientsExchange, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Success, actural.Status);
    Assert.NotNull(actural.Task);
    mockOnlineRobotsService.Verify(m => m.UpdateRobotDataAsync(RobotGuid, robotClientsExchange, false), Times.Once);
    mockOnlineRobotsService.Verify(m => m.GetAutoTaskNextApi(RobotGuid), Times.Once);
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskNextConstructAsync(It.IsAny<AutoTask>()), Times.Never);
    mockAutoTaskSchedulerService.Verify(m => m.GetAutoTaskAsync(RobotGuid), Times.Once);
    mockOnlineRobotsService.Verify(m => m.GetRobotCommands(RobotGuid), Times.Once);
  }

  [Fact]
  public async Task Exchange_CalledWithRunningRobot_ShouldReturnRobotClientsRespond()
  {
    // Arrange
    RobotClientsExchange localRobotClientsExchange = new() {
      RobotStatus = RobotClientsRobotStatus.Running,
      CriticalStatus = new RobotClientsRobotCriticalStatus {
        HardwareEmergencyStop = true,
        SoftwareEmergencyStop = true,
      },
      Position = new RobotClientsDof {
        X = 1,
        Y = 2,
        Rotation = 3,
      },
      NavProgress = new RobotClientsAutoTaskNavProgress {
        Eta = 1,
        Recoveries = 2,
        DistanceRemaining = 3,
        WaypointsRemaining = 4,
      }
    };
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Exchange), RobotGuid.ToString());
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Exchange(localRobotClientsExchange, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Success, actural.Status);
    Assert.Null(actural.Task);
    mockOnlineRobotsService.Verify(m => m.UpdateRobotDataAsync(RobotGuid, localRobotClientsExchange, false), Times.Once);
    mockOnlineRobotsService.Verify(m => m.GetAutoTaskNextApi(RobotGuid), Times.Once);
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskNextConstructAsync(It.IsAny<AutoTask>()), Times.Never);
    mockAutoTaskSchedulerService.Verify(m => m.GetAutoTaskAsync(RobotGuid), Times.Never);
    mockOnlineRobotsService.Verify(m => m.GetRobotCommands(RobotGuid), Times.Once);
  }

  [Fact]
  public async Task Exchange_CalledWithApiTask_ShouldReturnRobotClientsRespond()
  {
    // Arrange
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.Exchange), RobotGuid.ToString());
    mockOnlineRobotsService.Setup(m => m.GetAutoTaskNextApi(RobotGuid)).Returns(new AutoTask {
      Id = 1,
      Name = "Task",
      Priority = 0,
      FlowId = 1,
      RealmId = 1,
      AssignedRobotId = RobotGuid,
      CurrentProgressId = (int)ProgressState.Moving,
    });
    mockAutoTaskSchedulerService.Setup(m => m.AutoTaskNextConstructAsync(It.IsAny<AutoTask>())).ReturnsAsync(robotClientsAutoTask);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.Exchange(robotClientsExchange, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Success, actural.Status);
    Assert.NotNull(actural.Task);
    mockOnlineRobotsService.Verify(m => m.UpdateRobotDataAsync(RobotGuid, robotClientsExchange, false), Times.Once);
    mockOnlineRobotsService.Verify(m => m.GetAutoTaskNextApi(RobotGuid), Times.Once);
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskNextConstructAsync(It.IsAny<AutoTask>()), Times.Once);
    mockAutoTaskSchedulerService.Verify(m => m.GetAutoTaskAsync(RobotGuid), Times.Never);
    mockOnlineRobotsService.Verify(m => m.GetRobotCommands(RobotGuid), Times.Once);
  }

  [Fact]
  public async Task AutoTaskNext_Called_ShouldReturnRobotClientsAutoTaskRespond()
  {
    // Arrange
    var nextToken = new RobotClientsNextToken {
      TaskId = 1,
      NextToken = "NextToken"
    };
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.AutoTaskNext), RobotGuid.ToString());
    mockAutoTaskSchedulerService.Setup(m => m.AutoTaskNextAsync(RobotGuid, It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(robotClientsAutoTask);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.AutoTaskNext(nextToken, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Success, actural.Status);
    Assert.NotNull(actural.Task);
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskNextAsync(RobotGuid, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    mockOnlineRobotsService.Verify(m => m.GetRobotCommands(RobotGuid), Times.Once);
  }

  [Fact]
  public async Task AutoTaskNext_CalledWithInvalidNextToken_ShouldReturnRobotClientsAutoTaskErrorRespond()
  {
    // Arrange
    var nextToken = new RobotClientsNextToken {
      TaskId = 1,
      NextToken = "NextToken"
    };
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.AutoTaskNext), RobotGuid.ToString());
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.AutoTaskNext(nextToken, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Failed, actural.Status);
    Assert.Null(actural.Task);
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskNextAsync(RobotGuid, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    mockOnlineRobotsService.Verify(m => m.GetRobotCommands(RobotGuid), Times.Once);
  }

  [Fact]
  public async Task AutoTaskAbort_Called_ShouldReturnRobotClientsRespond()
  {
    // Arrange
    var abortToken = new RobotClientsAbortToken {
      TaskId = 1,
      NextToken = "NextToken",
      AbortReason = RobotClientsAbortReason.UserApi
    };
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.AutoTaskAbort), RobotGuid.ToString());
    mockAutoTaskSchedulerService.Setup(m => m.AutoTaskAbortAsync(RobotGuid, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<AutoTaskAbortReason>())).ReturnsAsync(robotClientsAutoTask);
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.AutoTaskAbort(abortToken, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Success, actural.Status);
    Assert.NotNull(actural.Task);
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskAbortAsync(RobotGuid, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<AutoTaskAbortReason>()), Times.Once);
    mockOnlineRobotsService.Verify(m => m.GetRobotCommands(RobotGuid), Times.Once);
  }

  [Fact]
  public async Task AutoTaskAbort_CalledWithInvalidAbortToken_ShouldReturnRobotClientsAutoTaskErrorRespond()
  {
    // Arrange
    var abortToken = new RobotClientsAbortToken {
      TaskId = 1,
      NextToken = "NextToken",
      AbortReason = RobotClientsAbortReason.UserApi
    };
    var serverCallContext = GenerateServerCallContext(nameof(RobotClientsService.AutoTaskAbort), RobotGuid.ToString());
    var robotClientsService = new RobotClientsService(mockAutoTaskSchedulerService.Object, mockEventService.Object, mockOnlineRobotsService.Object, mockConfiguration.Object,mockRobotService.Object);

    // Act
    var actural = await robotClientsService.AutoTaskAbort(abortToken, serverCallContext);

    // Assert
    Assert.NotNull(actural);
    Assert.Equal(RobotClientsResultStatus.Failed, actural.Status);
    mockAutoTaskSchedulerService.Verify(m => m.AutoTaskAbortAsync(RobotGuid, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<AutoTaskAbortReason>()), Times.Once);
    mockOnlineRobotsService.Verify(m => m.GetRobotCommands(RobotGuid), Times.Once);
  }
}