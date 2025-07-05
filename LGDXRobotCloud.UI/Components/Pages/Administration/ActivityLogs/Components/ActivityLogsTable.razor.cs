using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Components.Shared.Table;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using static LGDXRobotCloud.UI.Client.Administration.ActivityLogs.ActivityLogsRequestBuilder;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.ActivityLogs.Components;

public sealed partial class ActivityLogsTable : AbstractTable
{
  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
  
  [Parameter]
  public string? DefaultEntityName { get; set; }

  [Parameter]
  public string? DefaultEntityId { get; set; }

  [Parameter]
  public string? ReturnUrl { get; set; }

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  private List<ActivityLogListDto>? ActivityLogs { get; set; }
  private bool HideOptions = false;
  private string? CurrentEntityName { get; set; }
  TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

  private static List<string> EntityNames { get; set; } = [
    "Email",
    nameof(Data.Entities.ApiKey),
    nameof(Data.Entities.AutoTask),
    nameof(Data.Entities.Flow),
    nameof(Data.Entities.LgdxRole),
    nameof(Data.Entities.LgdxUser),
    nameof(Data.Entities.Progress),
    nameof(Data.Entities.Realm),
    nameof(Data.Entities.Robot),
    nameof(Data.Entities.Trigger),
    nameof(Data.Entities.Waypoint)
  ];

  private async Task HandleEntityCategoryChange(object? args)
  {
    CurrentPage = 1;
    if (args != null)
    {
      CurrentEntityName = args.ToString();
    }
    else
    {
      CurrentEntityName = null;
    }
    await Refresh();
  }

  private static string DisplayUser(LgdxUserSearchDto? user)
  {
    if (user == null)
      return "System";
    if (user.UserName != null)
      return user.UserName;
    return "Deleted User";
  }

  private static string DisplaEntityId(ActivityLogListDto activityLogListDto)
  {
    if (activityLogListDto.EntityName!.Equals("Email", StringComparison.OrdinalIgnoreCase))
    {
      return ((EmailType)int.Parse(activityLogListDto.EntityId!)).ToEnumMember() ?? "";
    }
    else
    {
      return activityLogListDto.EntityId ?? "";
    }
  }

  private string DisplayViewUrl(string url)
  {
    if (string.IsNullOrWhiteSpace(ReturnUrl))
    {
      return url;
    }
    else
    {
      return $"{url}?ReturnUrl={ReturnUrl}";
    }
  }

  public override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    ActivityLogs = await LgdxApiClient.Administration.ActivityLogs.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new ActivityLogsRequestBuilderGetQueryParameters
      {
        EntityName = CurrentEntityName,
        EntityId = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    ActivityLogs = await LgdxApiClient.Administration.ActivityLogs.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new ActivityLogsRequestBuilderGetQueryParameters
      {
        EntityName = CurrentEntityName,
        EntityId = DataSearch,
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
    if (pageNum == CurrentPage)
      return;
    CurrentPage = pageNum;
    if (pageNum > PaginationHelper?.PageCount || pageNum < 1)
      return;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    ActivityLogs = await LgdxApiClient.Administration.ActivityLogs.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new ActivityLogsRequestBuilderGetQueryParameters
      {
        EntityName = CurrentEntityName,
        EntityId = DataSearch,
        PageNumber = pageNum,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && ActivityLogs?.Count == 1)
      CurrentPage--;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    ActivityLogs = await LgdxApiClient.Administration.ActivityLogs.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new ActivityLogsRequestBuilderGetQueryParameters
      {
        EntityName = CurrentEntityName,
        EntityId = DataSearch,
        PageNumber = CurrentPage,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }
  
  protected override void OnInitialized()
  {
    var user = AuthenticationStateProvider.GetAuthenticationStateAsync().Result.User;
    var settings = TokenService.GetSessionSettings(user);
    TimeZone = settings.TimeZone;
    OnInitializedAsync();
  }
  
  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(DefaultEntityName), out var _entityName))
    {
      CurrentEntityName = _entityName;
      HideOptions = true;
    }
    if (parameters.TryGetValue<string?>(nameof(DefaultEntityId), out var _entityId))
    {
      if (!string.IsNullOrWhiteSpace(_entityId))
      {
        DataSearch = _entityId;
        HideOptions = true;
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}
