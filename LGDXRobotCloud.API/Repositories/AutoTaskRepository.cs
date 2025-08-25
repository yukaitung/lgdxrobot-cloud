using System.Text.Json;
using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Enums;
using LGDXRobotCloud.Utilities.Helpers;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using StackExchange.Redis;
using static StackExchange.Redis.RedisChannel;

namespace LGDXRobotCloud.API.Repositories;

public interface IAutoTaskRepository
{
  Task<Guid?> SchedulerHoldAnyRobotAsync(int realmId);
  Task<bool> SchedulerHoldRobotAsync(int realmId, Guid robotId);
  Task SchedulerReleaseRobotAsync(int realmId, Guid robotId);
  Task AddAutoTaskAsync(int realmId, Guid robotId, RobotClientsAutoTask autoTask);
  Task AutoTaskHasUpdateAsync(int realmId, AutoTaskUpdate autoTaskUpdate);
}

public class AutoTaskRepository(
    IConnectionMultiplexer redisConnection,
    IRobotDataRepository robotDataRepository
  ) : IAutoTaskRepository
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection;
  private readonly IRobotDataRepository _robotDataRepository = robotDataRepository;

  public async Task<Guid?> SchedulerHoldAnyRobotAsync(int realmId)
  {
    var db = _redisConnection.GetDatabase();
    int robotStatus = (int)RobotStatus.Idle;
    var search = await db.FT().SearchAsync(RedisHelper.GetRobotDataIndex(realmId),
      new Query($"@{nameof(RobotData.RobotStatus)}:[{robotStatus} {robotStatus}] @{nameof(RobotData.PauseTaskAssignment)}:{{false}}")
        .Limit(0, 1)
        .ReturnFields(["__key"]));
    string? robotId = search.Documents.FirstOrDefault()?.Id.Replace(RedisHelper.GetRobotDataPrefix(realmId), string.Empty);
    if (robotId == null)
    {
      return null;
    }
    bool result = await db.HashSetAsync(RedisHelper.GetSchedulerHold(realmId, Guid.Parse(robotId)), "Value", "1", When.NotExists);
    return result ? Guid.Parse(robotId) : null;
  }

  public async Task<bool> SchedulerHoldRobotAsync(int realmId, Guid robotId)
  {
    var robotData = await _robotDataRepository.GetRobotDataAsync(realmId, robotId);
    if (robotData == null ||
        robotData.RobotStatus != RobotStatus.Idle ||
        robotData.PauseTaskAssignment == true)
    {
      return false;
    }
    var db = _redisConnection.GetDatabase();
    return await db.HashSetAsync(RedisHelper.GetSchedulerHold(realmId, robotId), "Value", "1", When.NotExists);
  }

  public async Task SchedulerReleaseRobotAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    await db.KeyDeleteAsync(RedisHelper.GetSchedulerHold(realmId, robotId));
  }

  public async Task AddAutoTaskAsync(int realmId, Guid robotId, RobotClientsAutoTask autoTask)
  {
    var subscriber = _redisConnection.GetSubscriber();
    var data = new RobotClientsResponse { Task = autoTask };
    var base64 = SerialiserHelper.ToBase64(data);
    await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetRobotExchangeQueue(robotId), PatternMode.Literal), base64);
  }

  public async Task AutoTaskHasUpdateAsync(int realmId, AutoTaskUpdate autoTaskUpdate)
  {
    var subscriber = _redisConnection.GetSubscriber();
    var json = JsonSerializer.Serialize(autoTaskUpdate);
    await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetAutoTaskUpdateQueue(realmId), PatternMode.Literal), json);
  }
}