using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Helpers;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using static StackExchange.Redis.RedisChannel;

namespace LGDXRobotCloud.API.Repositories;

public interface ISlamDataRepository
{
  Task<bool> StartSlamAsync(int realmId, Guid robotId);
  Task StopSlamAsync(int realmId, Guid robotId);
  Task SetSlamExchangeAsync(int realmId, SlamData exchange);
  Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands);
}

public partial class SlamDataRepository(
    IConnectionMultiplexer redisConnection,
    ILogger<SlamDataRepository> logger,
    IRobotDataRepository robotDataRepository
  ) : ISlamDataRepository
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
  private readonly IRobotDataRepository _robotDataRepository = robotDataRepository ?? throw new ArgumentNullException(nameof(robotDataRepository));

  [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Redis SlamDataRepository Exception: {Msg}")]
  public partial void LogException(string msg);

  public async Task<bool> StartSlamAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    try
    {
      bool result = await db.JSON().SetAsync(RedisHelper.GetSlamData(realmId), "$", new SlamData(), When.NotExists);
      if (!result)
      {
        // Only one robot can running SLAM at a time
        return false;
      }
      await db.KeyExpireAsync(RedisHelper.GetSlamData(realmId), TimeSpan.FromMinutes(5));
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
      return false;
    }
    await _robotDataRepository.StartExchangeAsync(realmId, robotId);
    return true;
  }

  public async Task StopSlamAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    try
    {
      await db.KeyDeleteAsync(RedisHelper.GetSlamData(realmId));
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
    }
    await _robotDataRepository.StopExchangeAsync(realmId, robotId);
  }

  public async Task SetSlamExchangeAsync(int realmId, SlamData exchange)
  {
    var db = _redisConnection.GetDatabase();
    var pipeline = new Pipeline(db);
    List<Task> tasks = [];
    try
    {
      tasks.Add(pipeline.Json.SetAsync(RedisHelper.GetSlamData(realmId), "$", exchange));
      tasks.Add(db.KeyExpireAsync(RedisHelper.GetSlamData(realmId), TimeSpan.FromMinutes(5)));
      pipeline.Execute();
      await Task.WhenAll(tasks);
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
    }
  }

  public async Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands)
  {
    var db = _redisConnection.GetDatabase();
    try
    {
      if (!await db.KeyExistsAsync(RedisHelper.GetSlamData(realmId)))
        return false;
      var subscriber = _redisConnection.GetSubscriber();
      var base64 = SerialiserHelper.ToBase64(commands);
      await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetSlamExchangeQueue(realmId), PatternMode.Literal), base64);
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
      return false;
    }
    return true;
  }
}