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

public partial class AutoTaskRepository(
    IConnectionMultiplexer redisConnection,
    ILogger<AutoTaskRepository> logger,
    IRobotDataRepository robotDataRepository
  ) : IAutoTaskRepository
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
  private readonly IRobotDataRepository _robotDataRepository = robotDataRepository ?? throw new ArgumentNullException(nameof(robotDataRepository));

  [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Redis AutoTaskRepository Exception: {Msg}")]
  public partial void LogException(string msg);

  public async Task<Guid?> SchedulerHoldAnyRobotAsync(int realmId)
  {
    var db = _redisConnection.GetDatabase();
    Guid? result = null;
    int robotStatus = (int)RobotStatus.Idle;
    try
    {
      var search = await db.FT().SearchAsync(RedisHelper.GetRobotDataIndex(realmId),
      new Query($"@{nameof(RobotData.RobotStatus)}:[{robotStatus} {robotStatus}] @{nameof(RobotData.PauseTaskAssignment)}:{{false}}")
          .Limit(0, 1)
          .ReturnFields(["__key"]));
      string? robotId = search.Documents.FirstOrDefault()?.Id.Replace(RedisHelper.GetRobotDataPrefix(realmId), string.Empty);
      if (robotId == null)
      {
        return null;
      }
      if (await db.HashSetAsync(RedisHelper.GetSchedulerHold(realmId, Guid.Parse(robotId)), "Value", "1", When.NotExists))
      {
        result = Guid.Parse(robotId);
      }
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
    }
    return result;
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

    bool result = false;
    var db = _redisConnection.GetDatabase();
    try
    {
      result = await db.HashSetAsync(RedisHelper.GetSchedulerHold(realmId, robotId), "Value", "1", When.NotExists);
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
    }
    return result;
  }

  public async Task SchedulerReleaseRobotAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    try
    {
      await db.KeyDeleteAsync(RedisHelper.GetSchedulerHold(realmId, robotId));
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
    }
  }

  public async Task AddAutoTaskAsync(int realmId, Guid robotId, RobotClientsAutoTask autoTask)
  {
    var subscriber = _redisConnection.GetSubscriber();
    var data = new RobotClientsResponse { Task = autoTask };
    var base64 = SerialiserHelper.ToBase64(data);
    try
    {
      await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetRobotExchangeQueue(robotId), PatternMode.Literal), base64);
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
    }
  }

  public async Task AutoTaskHasUpdateAsync(int realmId, AutoTaskUpdate autoTaskUpdate)
  {
    var subscriber = _redisConnection.GetSubscriber();
    var json = JsonSerializer.Serialize(autoTaskUpdate);
    try
    {
      await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetAutoTaskUpdateQueue(realmId), PatternMode.Literal), json);
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
    }
  }
}