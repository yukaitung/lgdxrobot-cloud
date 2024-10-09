using System.Runtime.Serialization;

namespace LGDXRobot2Cloud.Utilities.Enums;

public enum AutoTaskNextController
{
  [EnumMember(Value = "Robot")]
  Robot = 1,

  [EnumMember(Value = "API")]
  API = 2
}
