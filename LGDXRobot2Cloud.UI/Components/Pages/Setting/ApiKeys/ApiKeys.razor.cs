namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.ApiKeys;

public sealed partial class ApiKeys
{
  public readonly List<string> Tabs = ["LGDXRobot2 API Keys", "Third-Party API Keys"];
  private int CurrentTab { get; set; } = 0;

  public void HandleTabChange(int index)
  {
    CurrentTab = index;
  }
}