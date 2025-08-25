using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Helpers;
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

  Task<RobotData?> GetRobotDataAsync(int realmId, Guid robotId);
  Task<bool> SetRobotDataAsync(int realmId, Guid robotId, RobotData data);

  Task<bool> AddRobotCommandAsync(int realmId, Guid robotId, RobotClientsRobotCommands cmd);
  
  // Slam
  Task<bool> StartSlamAsync(int realmId, Guid robotId);
  Task StopSlamAsync(int realmId, Guid robotId);
  Task<bool> SetSlamExchangeAsync(int realmId, SlamData exchange);

  Task<bool> AddSlamCommandAsync(int realmId, RobotClientsSlamCommands commands);
}

public class RobotDataRepository(
    IConnectionMultiplexer redisConnection
  ) : IRobotDataRepository
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection;

  private async Task<bool> IndexExistsAsync(string indexName)
  {
    var db = _redisConnection.GetDatabase();
    try
    {
      var index = await db.FT().InfoAsync(indexName);
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("Unknown index name", StringComparison.CurrentCultureIgnoreCase))
      {
        return false;
      }
      throw;
    }
    return true;
  }

  public async Task StartExchangeAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();

    // Create index
    if (!await IndexExistsAsync(RedisHelper.GetRobotDataIndex(realmId)))
    {
      try
      {
        await db.FT().CreateAsync(RedisHelper.GetRobotDataIndex(realmId),
          new FTCreateParams()
            .On(IndexDataType.JSON)
            .Prefix(RedisHelper.GetRobotDataPrefix(realmId)),
          new Schema()
          .AddNumericField(new FieldName($"$.{nameof(RobotData.RobotStatus)}", $"{nameof(RobotData.RobotStatus)}"))
          .AddTagField(new FieldName($"$.{nameof(RobotData.PauseTaskAssignment)}", $"{nameof(RobotData.PauseTaskAssignment)}")));
      }
      catch (Exception ex)
      {
        if (!ex.Message.Contains("Index already exists", StringComparison.CurrentCultureIgnoreCase))
        {
          throw;
        }
      }
    }

    await db.JSON().SetAsync(RedisHelper.GetRobotData(realmId, robotId), "$", new RobotData());
  }

  public async Task StopExchangeAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    await db.KeyDeleteAsync(RedisHelper.GetRobotData(realmId, robotId));
  }

  public async Task<RobotData?> GetRobotDataAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    return await db.JSON().GetAsync<RobotData>(RedisHelper.GetRobotData(realmId, robotId));
  }

  public async Task<bool> SetRobotDataAsync(int realmId, Guid robotId, RobotData data)
  {
    var db = _redisConnection.GetDatabase();
    return await db.JSON().SetAsync(RedisHelper.GetRobotData(realmId, robotId), "$", data);
  }

  public async Task<bool> AddRobotCommandAsync(int realmId, Guid robotId, RobotClientsRobotCommands cmd)
  {
    var db = _redisConnection.GetDatabase();
    if (!await db.KeyExistsAsync(RedisHelper.GetRobotData(realmId, robotId)))
      return false;

    var subscriber = _redisConnection.GetSubscriber();
    var data = new RobotClientsResponse { Commands = cmd };
    var base64 = SerialiserHelper.ToBase64(data);
    await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetRobotExchangeQueue(robotId), PatternMode.Literal), base64);
    return true;
  }

  public async Task<bool> StartSlamAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    bool result = await db.HashSetAsync(RedisHelper.GetSlamRobot(realmId), "RobotId", robotId.ToString(), When.NotExists);
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
    await db.KeyDeleteAsync(RedisHelper.GetSlamRobot(realmId));
    await StopExchangeAsync(realmId, robotId);
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