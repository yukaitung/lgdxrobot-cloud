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

public class SlamDataRepository(
    IConnectionMultiplexer redisConnection,
    IRobotDataRepository robotDataRepository
  ) : ISlamDataRepository
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection;
  private readonly IRobotDataRepository _robotDataRepository = robotDataRepository;

  public async Task<bool> StartSlamAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    bool result = await db.JSON().SetAsync(RedisHelper.GetSlamData(realmId), "$", new SlamData(), When.NotExists);
    if (!result)
    {
      // Only one robot can running SLAM at a time
      return false;
    }
    await db.KeyExpireAsync(RedisHelper.GetSlamData(realmId), TimeSpan.FromMinutes(5));
    await _robotDataRepository.StartExchangeAsync(realmId, robotId);
    return true;
  }

  public async Task StopSlamAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    await db.KeyDeleteAsync(RedisHelper.GetSlamData(realmId));
    await _robotDataRepository.StopExchangeAsync(realmId, robotId);
  }

  public async Task SetSlamExchangeAsync(int realmId, SlamData exchange)
  {
    var db = _redisConnection.GetDatabase();
    var pipeline = new Pipeline(db);
    List<Task> tasks = [];
    tasks.Add(pipeline.Json.SetAsync(RedisHelper.GetSlamData(realmId), "$", exchange));
    tasks.Add(db.KeyExpireAsync(RedisHelper.GetSlamData(realmId), TimeSpan.FromMinutes(5)));
    pipeline.Execute();
    await Task.WhenAll(tasks);
  }

  public async Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands)
  {
    var db = _redisConnection.GetDatabase();
    if (!await db.KeyExistsAsync(RedisHelper.GetSlamData(realmId)))
      return false;
    var subscriber = _redisConnection.GetSubscriber();
    var base64 = SerialiserHelper.ToBase64(commands);
    await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetSlamExchangeQueue(realmId), PatternMode.Literal), base64);
    return true;
  }
}