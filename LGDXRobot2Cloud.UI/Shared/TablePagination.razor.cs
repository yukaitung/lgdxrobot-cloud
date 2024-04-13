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
     
    private async Task ChangePage(MouseEventArgs e, int pageNum)
    {
      await OnPageChange.InvokeAsync(pageNum);
    }

    public override async Task SetParametersAsync(ParameterView parameters) 
    {
      if (parameters.TryGetValue<PaginationMetadata>(nameof(PaginationMetadata), out var value))
      {
        var pageCount = value.PageCount;
        var currentPage = value.CurrentPage;
        _renderPage.Clear();
        for(int i = currentPage - 3; i <= currentPage + 3; i++)
          if (i > 1 && i < pageCount) _renderPage.Add(i);
        _renderPage.Add(1);
        _renderPage.Add(pageCount);
      }
      await base.SetParametersAsync(parameters);
    }
  }
}