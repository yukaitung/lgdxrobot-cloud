using System.Text.Json;
using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Utilities.Helpers;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LGDXRobotCloud.UI.Services;

public interface ISlamDataService
{
  Task<SlamData?> GetSlamDataAsync(int realmId);
  Task<(RobotData?, SlamData?)> GetAllDataAsync(int realmId, Guid robotId);
}

public class SlamDataService(
    IConnectionMultiplexer redisConnection
  ) : ISlamDataService
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection;

  public async Task<SlamData?> GetSlamDataAsync(int realmId)
  {
    var db = _redisConnection.GetDatabase();
    var result = await db.JSON().GetAsync(RedisHelper.GetSlamData(realmId));
    var str = result.ToString();
    if (string.IsNullOrEmpty(str))
    {
      return null;
    }
    else
    {
      if (str.StartsWith('['))
      {
        str = str[1..^1];
      }
      var data = JsonSerializer.Deserialize<SlamData>(str);
      return data;
    }
  }

  public async Task<(RobotData?, SlamData?)> GetAllDataAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    RedisKey[] keys = [RedisHelper.GetRobotData(realmId, robotId), RedisHelper.GetSlamData(realmId)];
    var result = await db.JSON().MGetAsync(keys, "$");
    RobotData? robotData = null;
    var str = result[0].ToString();
    if (str.StartsWith('['))
    {
      str = str[1..^1];
    }
    if (str != string.Empty)
    {
      robotData = JsonSerializer.Deserialize<RobotData>(str);
    }
    SlamData? slamData = null;
    str = result[1].ToString();
    if (str.StartsWith('['))
    {
      str = str[1..^1];
    }
    if (str != string.Empty)
    {
      slamData = JsonSerializer.Deserialize<SlamData>(str);
    }
    return (robotData, slamData);
  }
}