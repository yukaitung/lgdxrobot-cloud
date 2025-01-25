using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Shared.Table;

public abstract class AbstractTable : ComponentBase
{
  protected PaginationHelper? PaginationHelper { get; set; }
  protected int CurrentPage { get; set; } = 1;
  protected int PageSize { get; set; } = 10;
  protected string DataSearch { get; set; } = string.Empty;
  protected string LastDataSearch { get; set; } = string.Empty;

  public abstract Task HandlePageSizeChange(int number);
  public abstract Task HandleSearch();
  public abstract Task HandleClearSearch();
  public abstract Task HandlePageChange(int pageNum);
  public abstract Task Refresh(bool deleteOpt = false);

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      await Refresh();
      StateHasChanged();
    }
    await base.OnAfterRenderAsync(firstRender);
  }
}