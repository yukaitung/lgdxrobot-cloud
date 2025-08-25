using System.Text.Json;
using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Utilities.Helpers;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using StackExchange.Redis;

namespace LGDXRobotCloud.UI.Services;

public interface IRobotDataService
{
  Task<Dictionary<Guid, RobotData>> GetRobotDataFromRealmAsync(int realmId);
  Task<Dictionary<Guid, RobotData?>> GetRobotDataFromListAsync(int realmId, List<Guid> robotIds);
}

public partial class RobotDataService(
    IConnectionMultiplexer redisConnection,
    ILogger<RobotDataService> logger
  ) : IRobotDataService
{
  private readonly IConnectionMultiplexer _redisConnection = redisConnection;

  [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "{Msg}")]
  public partial void LogException(string msg);

  public async Task<Dictionary<Guid, RobotData>> GetRobotDataFromRealmAsync(int realmId)
  {
    Dictionary<Guid, RobotData> robotData = [];
    var db = _redisConnection.GetDatabase();
    try
    {
      var search = await db.FT().SearchAsync(RedisHelper.GetRobotDataIndex(realmId), new Query($"*"));
      foreach (var item in search.Documents)
      {
        var data = JsonSerializer.Deserialize<RobotData>(item["json"]!);
        Guid id = Guid.Parse(item.Id.Replace(RedisHelper.GetRobotDataPrefix(realmId), string.Empty));
        robotData.Add(id, data!);
      }
    }
    catch (Exception ex)
    {
      if (!ex.Message.Contains("Unknown index name", StringComparison.CurrentCultureIgnoreCase))
      {
        LogException(ex.Message);
      }
    }
    return robotData;
  }

  public async Task<Dictionary<Guid, RobotData?>> GetRobotDataFromListAsync(int realmId, List<Guid> robotIds)
  {
    if (robotIds.Count == 0)
      return [];

    RedisKey[] keys = new RedisKey[robotIds.Count];
    for (int i = 0; i < robotIds.Count; i++)
    {
      keys[i] = RedisHelper.GetRobotData(realmId, robotIds[i]);
    }
    var db = _redisConnection.GetDatabase();
    RedisResult[] result = [];
    try
    {
      result = await db.JSON().MGetAsync(keys, "$");
    }
    catch (Exception ex)
    {
      LogException(ex.Message);
    }

    Dictionary<Guid, RobotData?> robotData = [];
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
        var data = JsonSerializer.Deserialize<RobotData>(str);
        robotData.Add(robotIds[i], data);
      }
    }
    return robotData;
  }
}