using System.Runtime.Serialization;

namespace LGDXRobot2Cloud.Utilities.Enums;

public enum AutoTaskCatrgory
{
  [EnumMember(Value = "Template")]
  Template = 1,

  [EnumMember(Value = "Waiting")]
  Waiting = 2,

  [EnumMember(Value = "Completed")]
  Completed = 3,

  [EnumMember(Value = "Aborted")]
  Aborted = 4,

  [EnumMember(Value = "Running")]
  Running = 5
}