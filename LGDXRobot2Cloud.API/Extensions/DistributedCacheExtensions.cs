using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace LGDXRobot2Cloud.API.Extensions;

public static class DistributedCacheExtensions
{
  private static readonly JsonSerializerOptions serializerOptions = new()
  {
    PropertyNamingPolicy = null,
    WriteIndented = true,
    AllowTrailingCommas = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
  };

  public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
  {
    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, serializerOptions));
    await cache.SetAsync(key, bytes, options);
  }

  public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key)
  {
    var bytes = await cache.GetAsync(key);
    return bytes != null ? JsonSerializer.Deserialize<T>(bytes, serializerOptions) : default;
  }
}