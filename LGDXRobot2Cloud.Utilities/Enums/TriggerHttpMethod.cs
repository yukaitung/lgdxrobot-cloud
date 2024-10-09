using System.Runtime.Serialization;

namespace LGDXRobot2Cloud.Utilities.Enums;

public enum TriggerHttpMethod
{
  [EnumMember(Value = "GET")]
  Get = 1,

  [EnumMember(Value = "POST")]
  Post = 2,

  [EnumMember(Value = "PUT")]
  Put = 3,

  [EnumMember(Value = "PATCH")]
  Patch = 4,

  [EnumMember(Value = "DELETE")]
  Delete = 5
}