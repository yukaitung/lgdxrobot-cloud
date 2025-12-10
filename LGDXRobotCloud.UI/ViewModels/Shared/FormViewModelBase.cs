namespace LGDXRobotCloud.UI.ViewModels.Shared;

public class FormViewModelBase
{
  public bool IsSuccess { get; set; } = false;
  public Dictionary<string, string>? Errors { get; set; }

  public void ClearMessages()
  {
    IsSuccess = false;
    Errors = null;
  }
}