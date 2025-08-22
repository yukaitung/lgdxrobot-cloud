using System.Text.Json;
using LGDXRobotCloud.Protos;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using StackExchange.Redis;
using static StackExchange.Redis.RedisChannel;

namespace LGDXRobotCloud.API.Repositories;

public interface IRobotDataRepository
{
  // Exchange
  Task StartExchangeAsync(int realmId, Guid robotId);
  Task StopExchangeAsync(int realmId, Guid robotId);

  Task<RobotClientsData?> GetRobotDataAsync(int realmId, Guid robotId);
  Task<bool> SetRobotDataAsync(int realmId, Guid robotId, RobotClientsData data);

  Task<Guid?> SchedulerHoldAnyRobotAsync(int realmId);
  Task<bool> SchedulerHoldRobotAsync(int realmId, Guid robotId);
  Task SchedulerReleaseRobotAsync(int realmId, Guid robotId);

  Task<bool> AddRobotCommandAsync(int realmId, Guid robotId, RobotClientsRobotCommands cmd);
  Task<bool> AddAutoTaskAsync(int realmId, Guid robotId, RobotClientsAutoTask autoTask);

  // Slam
  Task<bool> StartSlamAsync(int realmId, Guid robotId);
  Task StopSlamAsync(int realmId, Guid robotId);

  Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands);
}

public class RobotDataRepository(
    IConnectionMultiplexer redisConnection
  ) : IRobotDataRepository
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection;

  public async Task StartExchangeAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();

    // Create index
    bool indexExists = await db.KeyExistsAsync($"robotClientData:{realmId}:*");
    if (!indexExists)
    {
      var schema = new Schema()
        .AddTextField(nameof(RobotClientsData.RobotId))
        .AddNumericField(nameof(RobotClientsData.RobotStatus))
        .AddNumericField(nameof(RobotClientsData.PauseTaskAssignment));

      await db.FT().CreateAsync($"idxRobotClientData:{realmId}",
        new FTCreateParams()
          .On(IndexDataType.JSON)
          .Prefix($"robotClientData:{realmId}:"),
        schema);
    }

    await db.JSON().SetAsync($"robotClientData:{realmId}:{robotId}", "$", new RobotClientsData());
  }

  public async Task StopExchangeAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    await db.KeyDeleteAsync($"robotClientData:{realmId}:{robotId}");
  }

  public async Task<RobotClientsData?> GetRobotDataAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    return await db.JSON().GetAsync<RobotClientsData>($"robotClientData:{realmId}:{robotId}");
  }

  public async Task<bool> SetRobotDataAsync(int realmId, Guid robotId, RobotClientsData data)
  {
    var db = _redisConnection.GetDatabase();
    return await db.JSON().SetAsync($"robotClientData:{realmId}:{robotId}", "$", data);
  }

  public async Task<Guid?> SchedulerHoldAnyRobotAsync(int realmId)
  {
    var db = _redisConnection.GetDatabase();
    var search = await db.FT().SearchAsync($"idxRobotClientData:{realmId}",
      new Query($"@{nameof(RobotClientsData.RobotStatus)}:{(int)RobotClientsRobotStatus.Idle} @{nameof(RobotClientsData.PauseTaskAssignment)}:0")
        .Limit(0, 1));
    string? robotId = search.Documents.Select(x => x[$"{nameof(RobotClientsData.RobotId)}"]).FirstOrDefault();
    if (robotId == null)
    {
      return null;
    }
    bool result = await db.HashSetAsync($"schedulerHold:{realmId}:{robotId}", "Value", "1", When.NotExists);
    return result ? Guid.Parse(robotId) : null;
  }

  public async Task<bool> SchedulerHoldRobotAsync(int realmId, Guid robotId)
  {
    var robotData = await GetRobotDataAsync(realmId, robotId);
    if (robotData == null ||
        robotData.RobotStatus != RobotClientsRobotStatus.Idle ||
        robotData.PauseTaskAssignment == true)
    {
      return false;
    }
    var db = _redisConnection.GetDatabase();
    return await db.HashSetAsync($"schedulerHold:{realmId}:{robotId}", "Value", "1", When.NotExists);
  }

  public async Task SchedulerReleaseRobotAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    await db.KeyDeleteAsync($"schedulerHold:{realmId}:{robotId}");
  }

  public async Task<bool> AddRobotCommandAsync(int realmId, Guid robotId, RobotClientsRobotCommands cmd)
  {
    var db = _redisConnection.GetDatabase();
    if (!await db.HashExistsAsync($"robotClientData:{realmId}:{robotId}", $"{nameof(RobotClientsData.RobotId)}"))
      return false;

    var subscriber = _redisConnection.GetSubscriber();
    var json = JsonSerializer.Serialize(cmd);
    await subscriber.PublishAsync(new RedisChannel($"robotCommandQueue:{robotId}", PatternMode.Literal), json);
    return true;
  }

  public async Task<bool> AddAutoTaskAsync(int realmId, Guid robotId, RobotClientsAutoTask autoTask)
  {
    var db = _redisConnection.GetDatabase();
    if (!await db.HashExistsAsync($"robotClientData:{realmId}:{robotId}", $"{nameof(RobotClientsData.RobotId)}"))
      return false;

    var subscriber = _redisConnection.GetSubscriber();
    var json = JsonSerializer.Serialize(autoTask);
    await subscriber.PublishAsync(new RedisChannel($"autoTaskQueue:{robotId}", PatternMode.Literal), json);
    return true;
  }

  public async Task<bool> StartSlamAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    bool result = await db.HashSetAsync($"slamRobot:{realmId}", "RobotId", robotId.ToString(), When.NotExists);
    if (!result)
    {
      // Only one robot can running SLAM at a time
      return false;
    }
    await StartExchangeAsync(realmId, robotId);
    return true;
  }

  public async Task StopSlamAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    await db.KeyDeleteAsync($"slamRobot:{realmId}");
    await StopExchangeAsync(realmId, robotId);
  }

  public async Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands)
  {
    var db = _redisConnection.GetDatabase();
    if (!await db.HashExistsAsync($"slamRobot:{realmId}", "RobotId"))
      return false;
    var subscriber = _redisConnection.GetSubscriber();
    var json = JsonSerializer.Serialize(commands);
    await subscriber.PublishAsync(new RedisChannel($"robotSlamCommandQueue:{realmId}", PatternMode.Literal), json);
    return true;
  }
}