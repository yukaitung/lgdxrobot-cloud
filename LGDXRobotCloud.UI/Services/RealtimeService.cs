using System.Collections.Concurrent;
using System.Text.Json;
using LGDXRobotCloud.Data.Contracts;
using StackExchange.Redis;
using static StackExchange.Redis.RedisChannel;

namespace LGDXRobotCloud.UI.Services;

public interface IRealTimeService
{
  Task SubscribeToTaskUpdateQueueAsync(int realmId, Action<AutoTaskUpdateContract> handler);
  Task UnsubscribeToTaskUpdateQueueAsync(int realmId, Action<AutoTaskUpdateContract> handler);
}

public class RealTimeService(
    IConnectionMultiplexer redisConnection
  ) : IRealTimeService
{
  private readonly ISubscriber _subscriber = redisConnection.GetSubscriber();

  private readonly ConcurrentDictionary<string, ConcurrentBag<Action<AutoTaskUpdateContract>>> autoTaskUpdateHandlers = [];

  public async Task SubscribeToTaskUpdateQueueAsync(int realmId, Action<AutoTaskUpdateContract> handler)
  {
    var queueName = $"autoTaskUpdate:{realmId}";
    var handlers = autoTaskUpdateHandlers.GetOrAdd(queueName, _ => []);
    handlers.Add(handler);

    // Only subscribe to Redis if it's the first handler
    if (handlers.Count == 1)
    {
      await _subscriber.SubscribeAsync(new RedisChannel(queueName, PatternMode.Literal), (channel, value) =>
      {
        if (autoTaskUpdateHandlers.TryGetValue(queueName, out var hd))
        {
          var update = JsonSerializer.Deserialize<AutoTaskUpdateContract>(value!);
          foreach (var h in hd)
          {
            // invoke all handlers
            h(update!);
          }
        }
      });
    }
  }

  public async Task UnsubscribeToTaskUpdateQueueAsync(int realmId, Action<AutoTaskUpdateContract> handler)
  {
    var queueName = $"autoTaskUpdate:{realmId}";
    if (autoTaskUpdateHandlers.TryGetValue(queueName, out var handlers))
    {
      var updatedHandlers = new ConcurrentBag<Action<AutoTaskUpdateContract>>(handlers.Where(h => h != handler));
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