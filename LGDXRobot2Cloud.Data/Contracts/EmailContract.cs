using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Data.Contracts;

public record EmailRecipient
{
  public required string Email { get; set; }
  public required string Name { get; set; }
}

public record EmailContract
{
  public required EmailType EmailType { get; set; }
  public required IEnumerable<EmailRecipient> Recipients { get; set; }
  public string? Metadata { get; set; }
}