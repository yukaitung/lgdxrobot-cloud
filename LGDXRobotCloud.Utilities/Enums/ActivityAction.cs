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

  [EnumMember(Value = "Read ApiKey Secret")]
  ApiKeySecretRead = 13,

  [EnumMember(Value = "Update ApiKey Secret")]
  ApiKeySecretUpdate = 14,

  [EnumMember(Value = "Manual Abort AutoTask")]
  AutoTaskManualAbort = 15,

  [EnumMember(Value = "Traffic Updated")]
  RealmTrafficUpdated = 16,


  // User Actions
  [EnumMember(Value = "Login Success")]
  LoginSuccess = 50,

  [EnumMember(Value = "Login Failed")]
  LoginFailed = 51,

  [EnumMember(Value = "User Password Reset")]
  UserPasswordReset = 52,

  [EnumMember(Value = "User Password Updated")]
  UserPasswordUpdated = 53,

  [EnumMember(Value = "User Two-Factor Authentication Enabled")]
  UserTwoFactorAuthenticationEnabled = 54,

  [EnumMember(Value = "User Two-Factor Authentication Disabled")]
  UserTwoFactorAuthenticationDisabled = 55,

  [EnumMember(Value = "User Unlocked")]
  UserUnlocked = 56,

  // Robot Actions
  [EnumMember(Value = "Robot Certificate Create")]
  RobotCertificateCreate = 100,

  [EnumMember(Value = "Robot Certificate Renew")]
  RobotCertificateRenew = 101,

  [EnumMember(Value = "Robot Stop Task Assignment")]
  RobotStopTaskAssignment = 102,

  [EnumMember(Value = "Robot Resume Task Assignment")]
  RobotResumeTaskAssignment = 103,

  [EnumMember(Value = "Robot Emergency Stop Enabled")]
  RobotEmergencyStopEnabled = 104,

  [EnumMember(Value = "Robot Emergency Stop Disabled")]
  RobotEmergencyStopDisabled = 105,
}