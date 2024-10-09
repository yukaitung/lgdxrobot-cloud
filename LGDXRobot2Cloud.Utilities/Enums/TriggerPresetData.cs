using System.Runtime.Serialization;

namespace LGDXRobot2Cloud.Utilities.Enums;

public enum TriggerPresetData
{
  [EnumMember(Value = "Task Id")]
  AutoTaskId = 1,

  [EnumMember(Value = "Task Name")]
  AutoTaskName = 2,

  [EnumMember(Value = "Progress Id")]
  AutoTaskCurrentProgressId = 3,

  [EnumMember(Value = "Progress Name")]
  AutoTaskCurrentProgressName = 4,

  [EnumMember(Value = "Robot Id")]
  RobotId = 5,

  [EnumMember(Value = "Robot Name")]
  RobotName = 6,

  [EnumMember(Value = "Robot Address")]
  RobotAddress = 7,

  [EnumMember(Value = "Robot Namespace")]
  RobotNamespace = 8
}