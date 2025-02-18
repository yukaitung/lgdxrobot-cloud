using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public enum AutoTaskNextController
{
  [EnumMember(Value = "Robot")]
  Robot = 1,

  [EnumMember(Value = "API")]
  API = 2
}
