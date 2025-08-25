using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Helpers;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using static StackExchange.Redis.RedisChannel;

namespace LGDXRobotCloud.API.Repositories;

public interface ISlamDataRepository
{
  Task<bool> StartSlamAsync(int realmId, Guid robotId);
  Task StopSlamAsync(int realmId, Guid robotId);
  Task<bool> SetSlamExchangeAsync(int realmId, SlamData exchange);
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
    bool result = await db.HashSetAsync(RedisHelper.GetSlamRobot(realmId), "RobotId", robotId.ToString(), When.NotExists);
    if (!result)
    {
      // Only one robot can running SLAM at a time
      return false;
    }
    await _robotDataRepository.StartExchangeAsync(realmId, robotId);
    return true;
  }

  public async Task StopSlamAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    await db.KeyDeleteAsync(RedisHelper.GetSlamRobot(realmId));
    await _robotDataRepository.StopExchangeAsync(realmId, robotId);
  }

  public async Task<bool> SetSlamExchangeAsync(int realmId, SlamData exchange)
  {
    var db = _redisConnection.GetDatabase();
    if (!await db.KeyExistsAsync(RedisHelper.GetSlamRobot(realmId)))
      return false;
    return await db.JSON().SetAsync(RedisHelper.GetSlamData(realmId), "$", exchange);
  }

  public async Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands)
  {
    var db = _redisConnection.GetDatabase();
    if (!await db.KeyExistsAsync(RedisHelper.GetSlamRobot(realmId)))
      return false;
    var subscriber = _redisConnection.GetSubscriber();
    var base64 = SerialiserHelper.ToBase64(commands);
    await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetSlamExchangeQueue(realmId), PatternMode.Literal), base64);
    return true;
  }
}