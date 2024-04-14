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

    private SortedSet<int> _renderPage { get; set; } = [];
    private int _lastIndex { get; set; } = 0;
     
    private async Task HandlePageChange(int pageNum)
    {
      await OnPageChange.InvokeAsync(pageNum);
    }

    public override async Task SetParametersAsync(ParameterView parameters) 
    {
      if (parameters.TryGetValue<PaginationMetadata>(nameof(PaginationMetadata), out var value))
      {
        var pageCount = value.PageCount;
        var currentPage = value.CurrentPage;
        var island = 5; // Odd number
        var min = currentPage < pageCount - island / 2 ? currentPage - island / 2 : pageCount - island; // Ensure everypage has 5 pages
        var max = currentPage + island / 2;
        _renderPage.Clear();
        for (int i = min; i <= max; i++)
        {
          if (i > 1 && i < pageCount) 
            _renderPage.Add(i);
          else if(max < pageCount) {
            max++;
          }
        }
        _renderPage.Add(1);
        _renderPage.Add(pageCount);
      }
      await base.SetParametersAsync(parameters);
    }
  }
}