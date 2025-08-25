namespace LGDXRobotCloud.UI.Components.Pages.Administration.ApiKeys;

public partial class ApiKeys
{
  public readonly List<string> Tabs = ["LGDXRobot Cloud API Keys", "Third-Party API Keys"];
  private int CurrentTab { get; set; } = 0;

  public void HandleTabChange(int index)
  {
    CurrentTab = index;
  }
}