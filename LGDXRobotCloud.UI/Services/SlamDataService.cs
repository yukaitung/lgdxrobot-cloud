using System.Text.Json;
using LGDXRobotCloud.Data.Contracts;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LGDXRobotCloud.UI.Services;

public interface ISlamDataService
{
  Task<SlamDataContract?> GetSlamDataAsync(int realmId);
  Task<(RobotDataContract?, SlamDataContract?)> GetAllDataAsync(int realmId, Guid robotId);
}

public class SlamDataService(
    IConnectionMultiplexer redisConnection
  ) : ISlamDataService
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection;

  public async Task<SlamDataContract?> GetSlamDataAsync(int realmId)
  {
    var db = _redisConnection.GetDatabase();
    var result = await db.JSON().GetAsync($"robotClientSlamData:{realmId}");
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
      var data = JsonSerializer.Deserialize<SlamDataContract>(str);
      return data;
    }
  }

  public async Task<(RobotDataContract?, SlamDataContract?)> GetAllDataAsync(int realmId, Guid robotId)
  {
    var db = _redisConnection.GetDatabase();
    RedisKey[] keys = [$"robotClientData:{realmId}:{robotId}", $"robotClientSlamData:{realmId}"];
    var result = await db.JSON().MGetAsync(keys, "$");
    RobotDataContract? robotData = null;
    var str = result[0].ToString();
    if (str.StartsWith('['))
    {
      str = str[1..^1];
    }
    if (str != string.Empty)
    {
      robotData = JsonSerializer.Deserialize<RobotDataContract>(str);
    }
    SlamDataContract? slamData = null;
    str = result[1].ToString();
    if (str.StartsWith('['))
    {
      str = str[1..^1];
    }
    if (str != string.Empty)
    {
      slamData = JsonSerializer.Deserialize<SlamDataContract>(str);
    }
    return (robotData, slamData);
  }
}