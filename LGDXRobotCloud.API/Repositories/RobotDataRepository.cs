using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Protos;
using LGDXRobotCloud.Utilities.Helpers;
using NRedisStack;
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
  Task SetRobotDataAsync(int realmId, Guid robotId, RobotData data);

  Task<bool> AddRobotCommandAsync(int realmId, Guid robotId, RobotClientsRobotCommands cmd);
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
    await db.KeyExpireAsync(RedisHelper.GetRobotData(realmId, robotId), TimeSpan.FromMinutes(5));
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

  public async Task SetRobotDataAsync(int realmId, Guid robotId, RobotData data)
  {
    var db = _redisConnection.GetDatabase();
    var pipeline = new Pipeline(db);
    List<Task> tasks = [];
    tasks.Add(pipeline.Json.SetAsync(RedisHelper.GetRobotData(realmId, robotId), "$", data));
    tasks.Add(db.KeyExpireAsync(RedisHelper.GetRobotData(realmId, robotId), TimeSpan.FromMinutes(5)));
    pipeline.Execute();
    await Task.WhenAll(tasks);
  }

  public async Task<bool> AddRobotCommandAsync(int realmId, Guid robotId, RobotClientsRobotCommands cmd)
  {
    var subscriber = _redisConnection.GetSubscriber();
    var data = new RobotClientsResponse { Commands = cmd };
    var base64 = SerialiserHelper.ToBase64(data);
    await subscriber.PublishAsync(new RedisChannel(RedisHelper.GetRobotExchangeQueue(robotId), PatternMode.Literal), base64);
    return true;
  }
}