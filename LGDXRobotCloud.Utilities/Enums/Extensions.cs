using System.Reflection;
using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public static class Extensions
{
  public static string? ToEnumMember<T>(this T value) where T : Enum
  {
    return typeof(T)
      .GetTypeInfo()
      .DeclaredMembers
      .SingleOrDefault(x => x.Name == value.ToString())?
      .GetCustomAttribute<EnumMemberAttribute>(false)?
      .Value;
  }
}