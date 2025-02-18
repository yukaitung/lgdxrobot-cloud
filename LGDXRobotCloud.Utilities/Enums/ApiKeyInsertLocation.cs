using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public enum ApiKeyInsertLocation
{
  [EnumMember(Value = "Header")]
  Header = 1,

  [EnumMember(Value = "Body")]
  Body = 2
}
