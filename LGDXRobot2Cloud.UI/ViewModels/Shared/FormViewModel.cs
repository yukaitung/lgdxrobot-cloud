namespace LGDXRobot2Cloud.UI.ViewModels.Shared;

public class FormViewModel
{
  public bool IsSuccess { get; set; } = false; 
  public Dictionary<string,string>? Errors { get; set; }
}