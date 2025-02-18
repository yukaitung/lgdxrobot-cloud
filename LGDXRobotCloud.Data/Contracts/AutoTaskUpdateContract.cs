namespace LGDXRobotCloud.Data.Contracts;

public record AutoTaskUpdateContract
{
  public required int Id { get; set; }

  public string? Name { get; set; }

  public required int Priority { get; set; }

  public required int FlowId { get; set; }

  public required string FlowName { get; set; }

  public required int RealmId { get; set; }

  public Guid? AssignedRobotId { get; set; }

  public string? AssignedRobotName { get; set; }

  public required int CurrentProgressId { get; set; }

  public required string CurrentProgressName { get; set; }
}