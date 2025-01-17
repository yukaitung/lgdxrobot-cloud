namespace LGDXRobot2Cloud.Data.Models.Business.Identity;

public record UpdatePasswordRequestBusinessModel
{
  public required string CurrentPassword { get; set; }
  
  public required string NewPassword { get; set; }
}