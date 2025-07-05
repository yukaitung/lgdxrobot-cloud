using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public enum EmailType
{
  [EnumMember (Value = "Welcome")]
  Welcome = 1,

  [EnumMember (Value = "Welcome With Password Setup")]
  WelcomePasswordSet = 2,

  [EnumMember (Value = "Password Reset")]
  PasswordReset = 3,

  [EnumMember (Value = "Password Update")]
  PasswordUpdate = 4,

  [EnumMember (Value = "Robot Stuck")]
  RobotStuck = 5,

  [EnumMember (Value = "Task Abort")]
  AutoTaskAbort = 6,

  [EnumMember (Value = "Trigger Failed")]
  TriggerFailed = 7,

  [EnumMember (Value = "Robot Certificate Expire")]
  RobotCertificateExipre = 8
}