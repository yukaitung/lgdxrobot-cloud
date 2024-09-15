using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Shared.Table;

public sealed partial class TablePagination
{
  [Parameter]
  public required PaginationHelper PaginationHelper { get; set; }

  [Parameter]
  public int ItemCount { get; set; }

  [Parameter]
  public EventCallback<int> OnPageChange { get; set; }

  private SortedSet<int> RenderPage { get; set; } = [];
  private int LastIndex { get; set; } = 0;
    
  public async Task HandlePageChange(int pageNum)
  {
    await OnPageChange.InvokeAsync(pageNum);
  }

  public override async Task SetParametersAsync(ParameterView parameters) 
  {
    if (parameters.TryGetValue<PaginationHelper>(nameof(PaginationHelper), out var value))
    {
      var pageCount = value.PageCount;
      var currentPage = value.CurrentPage;
      var island = 5; // Odd number
      var min = currentPage < pageCount - island / 2 ? currentPage - island / 2 : pageCount - island; // Ensure everypage has 5 pages
      var max = currentPage + island / 2;
      RenderPage.Clear();
      for (int i = min; i <= max; i++)
      {
        if (i > 1 && i < pageCount) 
          RenderPage.Add(i);
        else if(max < pageCount)
          max++;
      }
      RenderPage.Add(1);
      RenderPage.Add(pageCount);
    }
    await base.SetParametersAsync(parameters);
  }
}
