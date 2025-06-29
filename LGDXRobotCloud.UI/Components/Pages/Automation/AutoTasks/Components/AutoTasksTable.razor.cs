using System.Threading.Tasks;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Components.Shared.Table;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using static LGDXRobotCloud.UI.Client.Automation.AutoTasks.AutoTasksRequestBuilder;

namespace LGDXRobotCloud.UI.Components.Pages.Automation.AutoTasks.Components;

public sealed partial class AutoTasksTable : AbstractTable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ICachedRealmService CachedRealmService { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public string Title { get; set; } = null!;

  public AutoTaskCatrgory? AutoTaskCatrgory { get; set; } = null;

  private int RealmId { get; set; }
  private List<AutoTaskListDto>? AutoTasks { get; set; } = [];

  public override async Task HandlePageSizeChange(int number)
  {
    if (AutoTaskCatrgory == null)
      return;
      
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        RealmId = RealmId,
        AutoTaskCatrgory = AutoTaskCatrgory.ToString(),
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public override async Task HandleSearch()
  {
    if (AutoTaskCatrgory == null)
      return;

    if (LastDataSearch == DataSearch)
      return;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        RealmId = RealmId,
        AutoTaskCatrgory = AutoTaskCatrgory.ToString(),
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
    LastDataSearch = DataSearch;
    CurrentPage = 1;
  }

  public override async Task HandleClearSearch()
  {
    if (DataSearch == string.Empty && LastDataSearch == string.Empty)
      return;
    DataSearch = string.Empty;
    await HandleSearch();
  }

  public override async Task HandlePageChange(int pageNum)
  {
    if (AutoTaskCatrgory == null)
      return;

    if (pageNum == CurrentPage)
      return;
    CurrentPage = pageNum;
    if (pageNum > PaginationHelper?.PageCount || pageNum < 1)
      return;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        RealmId = RealmId,
        AutoTaskCatrgory = AutoTaskCatrgory.ToString(),
        Name = DataSearch,
        PageNumber = pageNum,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (AutoTaskCatrgory == null)
      return;

    if (deleteOpt && CurrentPage > 1 && AutoTasks?.Count == 1)
      CurrentPage--;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    AutoTasks = await LgdxApiClient.Automation.AutoTasks.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new AutoTasksRequestBuilderGetQueryParameters {
        RealmId = RealmId,
        AutoTaskCatrgory = AutoTaskCatrgory.ToString(),
        Name = DataSearch,
        PageNumber = CurrentPage,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public async Task HandleTaskCategoryChange(object? args)
  {
    if (args != null)
    {
      CurrentPage = 1;
      if (int.TryParse(args.ToString()!, out int category))
      {
        AutoTaskCatrgory = (AutoTaskCatrgory)category;
        await Refresh();
      }
      else
        AutoTaskCatrgory = null;
    }
  }


  protected override async Task OnInitializedAsync()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    RealmId = settings.CurrentRealmId;
    await base.OnInitializedAsync();
  }
}