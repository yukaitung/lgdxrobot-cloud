using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Data.Contracts;

public record EmailContract
{
  public required EmailType EmailType { get; set; }
  public required string ReceipentName { get; set; }
  public required string ReceipentAddress { get; set; }
  public string? Metadata { get; set; }
}