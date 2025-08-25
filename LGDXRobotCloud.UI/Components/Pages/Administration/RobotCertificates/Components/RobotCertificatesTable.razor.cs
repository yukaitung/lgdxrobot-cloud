using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Components.Shared.Table;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using static LGDXRobotCloud.UI.Client.Administration.RobotCertificates.RobotCertificatesRequestBuilder;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.RobotCertificates.Components;
public partial class RobotCertificatesTable : AbstractTable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  private List<RobotCertificateListDto>? RobotCertificates { get; set; }
  TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
  
  public override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    RobotCertificates = await LgdxApiClient.Administration.RobotCertificates.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new RobotCertificatesRequestBuilderGetQueryParameters
      {
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
    RobotCertificates = await LgdxApiClient.Administration.RobotCertificates.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new RobotCertificatesRequestBuilderGetQueryParameters {
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
    RobotCertificates = await LgdxApiClient.Administration.RobotCertificates.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new RobotCertificatesRequestBuilderGetQueryParameters {
        PageNumber = pageNum,
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

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && RobotCertificates?.Count == 1)
      CurrentPage--;

    var headersInspectionHandlerOption = HeaderHelper.GenrateHeadersInspectionHandlerOption();
    RobotCertificates = await LgdxApiClient.Administration.RobotCertificates.GetAsync(x =>
    {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new RobotCertificatesRequestBuilderGetQueryParameters
      {
        PageNumber = CurrentPage,
        PageSize = PageSize
      };
    });
    PaginationHelper = HeaderHelper.GetPaginationHelper(headersInspectionHandlerOption);
  }
}
