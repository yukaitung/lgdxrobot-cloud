using LGDXRobot2Cloud.Shared.Models.Blazor;
using LGDXRobot2Cloud.Shared.Services;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;

namespace LGDXRobot2Cloud.UI.Components.Triggers
{
  public partial class TriggerTable : AbstractTable
  {
    [Inject]
    public required ITriggerService TriggerService { get; set; }

    [Parameter]
    public EventCallback<int> OnIdSelected { get; set; }

    private List<TriggerBlazor>? TriggersList { get; set; }
    private PaginationMetadata? PaginationMetadata { get; set; }
    private int CurrentPage { get; set; } = 1;
    private int PageSize { get; set; } = 10;
    private string DataSearch { get; set; } = string.Empty;
    private string LastDataSearch { get; set; } = string.Empty;
    
    protected override async Task HandlePageSizeChange(int number)
    {
      PageSize = number;
      if (PageSize > 100)
        PageSize = 100;
      else if (PageSize < 1)
        PageSize = 1;
      var data = await TriggerService.GetTriggersAsync(DataSearch, 1, PageSize);
      TriggersList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }

    protected override async Task HandleSearch()
    {
      if (LastDataSearch == DataSearch)
        return;
      var data = await TriggerService.GetTriggersAsync(DataSearch, 1, PageSize);
      TriggersList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
      LastDataSearch = DataSearch;
    }

    protected override async Task HandleClearSearch()
    {
      if (DataSearch == string.Empty && LastDataSearch == string.Empty)
        return;
      DataSearch = string.Empty;
      await HandleSearch();
    }

    protected override async Task HandleItemSelect(int id)
    {
      await OnIdSelected.InvokeAsync(id);
    }

    protected override async Task HandlePageChange(int pageNum)
    {
      if (pageNum == CurrentPage)
        return;
      CurrentPage = pageNum;
      if (pageNum > PaginationMetadata?.PageCount || pageNum < 1)
        return;
      var data = await TriggerService.GetTriggersAsync(DataSearch, pageNum, PageSize);
      TriggersList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }

    public override async Task Refresh(bool deleteOpt = false)
    {
      if (deleteOpt && CurrentPage > 1 && TriggersList?.Count == 1)
        CurrentPage--;
      var data = await TriggerService.GetTriggersAsync(DataSearch, CurrentPage, PageSize);
      TriggersList = data.Item1?.ToList();
      PaginationMetadata = data.Item2;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (firstRender)
      {
        await Refresh();
        StateHasChanged();
      }
    }
  }
}