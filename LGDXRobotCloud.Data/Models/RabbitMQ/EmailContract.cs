using LGDXRobotCloud.Utilities.Enums;

namespace LGDXRobotCloud.Data.Models.RabbitMQ;

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