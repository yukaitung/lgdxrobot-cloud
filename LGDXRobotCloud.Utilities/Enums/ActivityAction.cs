using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public enum ActivityAction
{
  // Basic CRUD
  [EnumMember(Value = "Create")]
  Create = 1,

  [EnumMember(Value = "Read")]
  Read = 2,

  [EnumMember(Value = "Update")]
  Update = 3,

  [EnumMember(Value = "Delete")]
  Delete = 4,

  // System Actions
  [EnumMember(Value = "Send Email")]
  SendEmail = 10,

  [EnumMember(Value = "Execute Trigger")]
  TriggerExecute = 11,

  [EnumMember(Value = "Retry Trigger")]
  TriggerRetry = 12,

  [EnumMember(Value = "Read Secret")]
  ApiKeySecretRead = 13,

  [EnumMember(Value = "Update Secret")]
  ApiKeySecretUpdate = 14,

  [EnumMember(Value = "Manual Abort")]
  AutoTaskManualAbort = 15,

  [EnumMember(Value = "Traffic Update")]
  RealmTrafficUpdated = 16,


  // User Actions
  [EnumMember(Value = "Login Success")]
  LoginSuccess = 50,

  [EnumMember(Value = "Login Failed")]
  LoginFailed = 51,

  [EnumMember(Value = "Password Reset")]
  UserPasswordReset = 52,

  [EnumMember(Value = "Password Update")]
  UserPasswordUpdated = 53,

  [EnumMember(Value = "Two-Factor Authentication Enable")]
  UserTwoFactorAuthenticationEnabled = 54,

  [EnumMember(Value = "Two-Factor Authentication Disable")]
  UserTwoFactorAuthenticationDisabled = 55,

  [EnumMember(Value = "Accont Unlock")]
  UserUnlocked = 56,

  // Robot Actions
  [EnumMember(Value = "Create Certificate")]
  RobotCertificateCreate = 100,

  [EnumMember(Value = "Renew Certificate")]
  RobotCertificateRenew = 101,

  [EnumMember(Value = "Stop Task Assignment")]
  RobotPauseTaskAssignmentEnabled = 102,

  [EnumMember(Value = "Resume Task Assignment")]
  RobotPauseTaskAssignmentDisabled = 103,

  [EnumMember(Value = "Emergency Stop Enable")]
  RobotSoftwareEmergencyStopEnabled = 104,

  [EnumMember(Value = "Emergency Stop Disable")]
  RobotSoftwareEmergencyStopDisabled = 105,
}