using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public enum LgdxRobotType
{
  [EnumMember(Value = "Custom Robot")]
  CustomRobot = 1,

  [EnumMember(Value = "LGDXRobot2")]
  LGDXRobot2Classic = 2,
}
