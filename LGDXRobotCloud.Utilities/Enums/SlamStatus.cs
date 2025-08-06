using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public enum SlamStatus
{
  [EnumMember(Value = "Idle")]
  Idle = 1,

  [EnumMember(Value = "Running")]
  Running = 2,

  [EnumMember(Value = "Success")]
  Success = 3,

  [EnumMember(Value = "Aborted")]
  Aborted = 4
}