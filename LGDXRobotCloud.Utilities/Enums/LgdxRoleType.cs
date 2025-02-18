using System.Runtime.Serialization;

namespace LGDXRobotCloud.Utilities.Enums;

public enum LgdxRoleType
{
  [EnumMember(Value = "Global Administrator")]
  GlobalAdministrator = 1,

  [EnumMember(Value = "Global Auditor")]
  GlobalAuditor = 2,

  [EnumMember(Value = "Robot Administrator")]
  RobotAdministrator = 3,

  [EnumMember(Value = "Robot Auditor")]
  RobotAuditor = 4,

  [EnumMember(Value = "Navigation Administrator")]
  NavigationAdministrator = 5,

  [EnumMember(Value = "Navigation Auditor")]
  NavigationAuditor = 6,

  [EnumMember(Value = "Automation Administrator")]
  AutomationAdministrator = 7,

  [EnumMember(Value = "Automation Auditor")]
  AutomationAuditor = 8,

  [EnumMember(Value = "Auto Task Operator")]
  AutoTaskAdministrator = 9,

  [EnumMember(Value = "Auto Task Auditor")]
  AutoTaskAuditor = 10,

  [EnumMember(Value = "Auto Task Operator")]
  AutoTaskOperator = 11,

  [EnumMember(Value = "Email Recipient")]
  EmailRecipient = 12
}