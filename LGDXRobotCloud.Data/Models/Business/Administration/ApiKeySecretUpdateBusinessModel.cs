namespace LGDXRobotCloud.Data.Models.Business.Administration;

public record ApiKeySecretUpdateBusinessModel
{
  public required string Secret { get; set; }
}