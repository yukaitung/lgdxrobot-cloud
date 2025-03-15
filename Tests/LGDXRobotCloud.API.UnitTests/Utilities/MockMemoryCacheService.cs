using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace LGDXRobotCloud.API.UnitTests.Utilities;

public static class MockMemoryCacheService 
{
  public static Mock<IMemoryCache> GetMemoryCache(object expectedValue, string cacheName = "") 
  {
    var mockMemoryCache = new Mock<IMemoryCache>();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
    if (string.IsNullOrEmpty(cacheName))
    {
      mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue)).Returns(true);
    }
    else
    {
      mockMemoryCache.Setup(x => x.TryGetValue(cacheName, out expectedValue)).Returns(true);
    }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);
    return mockMemoryCache;
  }
}