using System.Collections.Concurrent;
using System.Text.Json;
using LGDXRobotCloud.Data.Models.Redis;
using LGDXRobotCloud.Utilities.Helpers;
using StackExchange.Redis;
using static StackExchange.Redis.RedisChannel;

namespace LGDXRobotCloud.UI.Services;

public interface IRealTimeService
{
  Task SubscribeToTaskUpdateQueueAsync(int realmId, Action<AutoTaskUpdate> handler);
  Task UnsubscribeToTaskUpdateQueueAsync(int realmId, Action<AutoTaskUpdate> handler);
}

public class RealTimeService(
    IConnectionMultiplexer redisConnection
  ) : IRealTimeService
{
  private readonly ISubscriber _subscriber = redisConnection.GetSubscriber();

  private readonly ConcurrentDictionary<string, ConcurrentBag<Action<AutoTaskUpdate>>> autoTaskUpdateHandlers = [];

  public async Task SubscribeToTaskUpdateQueueAsync(int realmId, Action<AutoTaskUpdate> handler)
  {
    var queueName = RedisHelper.GetAutoTaskUpdateQueue(realmId);
    var handlers = autoTaskUpdateHandlers.GetOrAdd(queueName, _ => []);
    handlers.Add(handler);

    // Only subscribe to Redis if it's the first handler
    if (handlers.Count == 1)
    {
      await _subscriber.SubscribeAsync(new RedisChannel(queueName, PatternMode.Literal), (channel, value) =>
      {
        if (autoTaskUpdateHandlers.TryGetValue(queueName, out var hd))
        {
          var update = JsonSerializer.Deserialize<AutoTaskUpdate>((string)value!);
          foreach (var h in hd)
          {
            // invoke all handlers
            h(update!);
          }
        }
      });
    }
  }

  public async Task UnsubscribeToTaskUpdateQueueAsync(int realmId, Action<AutoTaskUpdate> handler)
  {
    var queueName = RedisHelper.GetAutoTaskUpdateQueue(realmId);
    if (autoTaskUpdateHandlers.TryGetValue(queueName, out var handlers))
    {
      var updatedHandlers = new ConcurrentBag<Action<AutoTaskUpdate>>(handlers.Where(h => h != handler));
      if (updatedHandlers.IsEmpty)
      {
        autoTaskUpdateHandlers.TryRemove(queueName, out _);
        await _subscriber.UnsubscribeAsync(new RedisChannel(queueName, PatternMode.Literal));
      }
      else
      {
        autoTaskUpdateHandlers[queueName] = updatedHandlers;
      }
    }
  }
}