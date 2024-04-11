using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Shared
{
  public partial class TableEntriesSelect
  {
    [Parameter]
    public EventCallback<int> OnChange { get; set; }
    
    private async Task ChangeSelect(ChangeEventArgs e)
    {
      var number = e.Value?.ToString() ?? "10";
      await OnChange.InvokeAsync(int.Parse(number));
    }
  }
}