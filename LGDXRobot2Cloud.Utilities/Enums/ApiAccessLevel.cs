using System.Runtime.Serialization;

namespace LGDXRobot2Cloud.Utilities.Enums;

public enum ApiAccessLevel
{
  [EnumMember(Value = "FullAccess")]
  FullAccess = 1,

  [EnumMember(Value = "Read")]
  Read = 2,

  [EnumMember(Value = "Write")]
  Write = 3,

  [EnumMember(Value = "Delete")]
  Delete = 4,
}