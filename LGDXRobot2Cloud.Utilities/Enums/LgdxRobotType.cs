using System.Runtime.Serialization;

namespace LGDXRobot2Cloud.Utilities.Enums;

public enum LgdxRobotType
{
  [EnumMember(Value = "LGDXRobot2 Classic")]
  LGDXRobot2Classic = 1,

  [EnumMember(Value = "Virtual Robot")]
  VirtualRobot = 2,

  [EnumMember(Value = "Custom Robot")]
  CustomRobot = 3,
}
