using System.Text.Json;
using LGDXRobotCloud.Data.Contracts;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using StackExchange.Redis;

namespace LGDXRobotCloud.UI.Services;

public interface IRobotDataService
{
  Task<Dictionary<Guid, RobotDataContract>> GetRobotDataFromRealmAsync(int realmId);
  Task<Dictionary<Guid, RobotDataContract?>> GetRobotDataFromListAsync(int realmId, List<Guid> robotIds);
}

public class RobotDataService(
    IConnectionMultiplexer redisConnection
  ) : IRobotDataService
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection;

  public async Task<Dictionary<Guid, RobotDataContract>> GetRobotDataFromRealmAsync(int realmId)
  {
    Dictionary<Guid, RobotDataContract> robotData = [];
    var db = _redisConnection.GetDatabase();
    try
    {
      var search = await db.FT().SearchAsync($"idxRobotClientData:{realmId}", new Query($"*"));
      foreach (var item in search.Documents)
      {
        var data = JsonSerializer.Deserialize<RobotDataContract>(item["json"]!);
        Guid id = Guid.Parse(item.Id.Replace($"robotClientData:{realmId}:", string.Empty));
        robotData.Add(id, data!);
      }
    }
    catch (Exception ex)
    {
      if (!ex.Message.Contains("Unknown index name", StringComparison.CurrentCultureIgnoreCase))
      {
        throw;
      }
    }
    return robotData;
  }

  public async Task<Dictionary<Guid, RobotDataContract?>> GetRobotDataFromListAsync(int realmId, List<Guid> robotIds)
  {
    if (robotIds.Count == 0)
      return [];

    RedisKey[] keys = new RedisKey[robotIds.Count];
    for (int i = 0; i < robotIds.Count; i++)
    {
      keys[i] = $"robotClientData:{realmId}:{robotIds[i]}";
    }
    var db = _redisConnection.GetDatabase();
    var result = await db.JSON().MGetAsync(keys, "$");

    Dictionary<Guid, RobotDataContract?> robotData = [];
    for (int i = 0; i < result.Length; i++)
    {
      var str = result[i].ToString();
      if (string.IsNullOrEmpty(str))
      {
        robotData.Add(robotIds[i], null);
      }
      else
      {
        if (str.StartsWith('['))
        {
          str = str[1..^1];
        }
        var data = JsonSerializer.Deserialize<RobotDataContract>(str);
        robotData.Add(robotIds[i], data);
      }
    }
    return robotData;
  }
}