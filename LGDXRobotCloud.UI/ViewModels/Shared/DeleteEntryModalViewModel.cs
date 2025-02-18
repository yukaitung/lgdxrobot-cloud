namespace LGDXRobotCloud.UI.ViewModels.Shared;

public record DeleteEntryModalViewModel
{
  public IDictionary<string,string>? Errors { get; set; }
  public bool IsReady { get; set; } = false;
}