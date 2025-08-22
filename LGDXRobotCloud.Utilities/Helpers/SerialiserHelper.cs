using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Helpers;

public static class SerialiserHelper
{
  public static string ToBase64<T>(T obj)
  {
    if (obj == null) throw new ArgumentNullException(nameof(obj));

    var serializer = new DataContractSerializer(typeof(T));
    using var ms = new MemoryStream();
    serializer.WriteObject(ms, obj);

    return Convert.ToBase64String(ms.ToArray());
  }

  public static T? FromBase64<T>(string base64)
  {
    if (string.IsNullOrWhiteSpace(base64)) throw new ArgumentNullException(nameof(base64));

    var bytes = Convert.FromBase64String(base64);
    var serializer = new DataContractSerializer(typeof(T));

    using var ms = new MemoryStream(bytes);
    return (T?)serializer.ReadObject(ms);
  }
}
