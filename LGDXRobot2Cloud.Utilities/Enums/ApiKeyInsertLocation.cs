using System.Runtime.Serialization;

namespace LGDXRobot2Cloud.Utilities.Enums;

public enum ApiKeyInsertLocation
{
  [EnumMember(Value = "Header")]
  Header = 1,

  [EnumMember(Value = "Body")]
  Body = 2
}
