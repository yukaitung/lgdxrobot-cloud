using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public enum TriggerPresetValue
{
  [EnumMember(Value = "Task ID")]
  AutoTaskId = 1,

  [EnumMember(Value = "Task Name")]
  AutoTaskName = 2,

  [EnumMember(Value = "Progress ID")]
  AutoTaskCurrentProgressId = 3,

  [EnumMember(Value = "Progress Name")]
  AutoTaskCurrentProgressName = 4,

  [EnumMember(Value = "Robot ID")]
  RobotId = 5,

  [EnumMember(Value = "Robot Name")]
  RobotName = 6,

  [EnumMember(Value = "Realm ID")]
  RealmId = 7,

  [EnumMember(Value = "Realm Name")]
  RealmName = 8,

  [EnumMember(Value = "Next Token")]
  NextToken = 9
}