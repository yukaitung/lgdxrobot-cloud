using LGDXRobot2Cloud.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LGDXRobot2Cloud.UI.Shared
{
  public partial class TablePagination
  {
    [Parameter]
    public required PaginationMetadata PaginationMetadata { get; set; }

    [Parameter]
    public int ItemCount { get; set; }

    [Parameter]
    public EventCallback<int> OnPageChange { get; set; }
    
    private async Task ChangePage(MouseEventArgs e, int pageNum)
    {
      await OnPageChange.InvokeAsync(pageNum);
    }
  }
}