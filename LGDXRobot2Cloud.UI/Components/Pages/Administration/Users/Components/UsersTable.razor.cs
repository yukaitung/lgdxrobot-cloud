using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Client.Models;
using LGDXRobot2Cloud.UI.Components.Shared.Table;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using static LGDXRobot2Cloud.UI.Client.Administration.Users.UsersRequestBuilder;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.Users.Components;

public sealed partial class UsersTable : AbstractTable
{
  [Inject]
  public required IUsersService UsersService { get; set; }

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  private List<LgdxUserListDto>? LgdxUsers { get; set; }

  private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  
  public override async Task HandlePageSizeChange(int number)
  {
    PageSize = number;
    if (PageSize > 100)
      PageSize = 100;
    else if (PageSize < 1)
      PageSize = 1;

    LgdxUsers = await LgdxApiClient.Administration.Users.GetAsync(
      x => x.QueryParameters = new UsersRequestBuilderGetQueryParameters {
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      }
    );
    var headersInspectionHandlerOption = new HeadersInspectionHandlerOption();
    var paginationHeader = headersInspectionHandlerOption.ResponseHeaders["X-Pagination"];
    PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>((Stream)paginationHeader, _jsonSerializerOptions);
  }

  public override async Task HandleSearch()
  {
    if (LastDataSearch == DataSearch)
      return;
    LastDataSearch = DataSearch;

    LgdxUsers = await LgdxApiClient.Administration.Users.GetAsync(
      x => x.QueryParameters = new UsersRequestBuilderGetQueryParameters {
        Name = DataSearch,
        PageNumber = 1,
        PageSize = PageSize
      }
    );
    var headersInspectionHandlerOption = new HeadersInspectionHandlerOption();
    var paginationHeader = headersInspectionHandlerOption.ResponseHeaders["X-Pagination"];
    PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>((Stream)paginationHeader, _jsonSerializerOptions);
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


    LgdxUsers = await LgdxApiClient.Administration.Users.GetAsync(
      x => x.QueryParameters = new UsersRequestBuilderGetQueryParameters {
        Name = DataSearch,
        PageNumber = pageNum,
        PageSize = PageSize
      }
    );
    var headersInspectionHandlerOption = new HeadersInspectionHandlerOption();
    var paginationHeader = headersInspectionHandlerOption.ResponseHeaders["X-Pagination"];
    PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>((Stream)paginationHeader, _jsonSerializerOptions);
  }

  public override async Task Refresh(bool deleteOpt = false)
  {
    if (deleteOpt && CurrentPage > 1 && LgdxUsers?.Count == 1)
      CurrentPage--;

    var headersInspectionHandlerOption = new HeadersInspectionHandlerOption()
    {
      InspectResponseHeaders = true
    };
    LgdxUsers = await LgdxApiClient.Administration.Users.GetAsync(x => {
      x.Options.Add(headersInspectionHandlerOption);
      x.QueryParameters = new UsersRequestBuilderGetQueryParameters {
        Name = DataSearch,
        PageNumber = CurrentPage,
        PageSize = PageSize
      };
    });
    var paginationHeader = headersInspectionHandlerOption.ResponseHeaders["X-Pagination"].FirstOrDefault();
    PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(paginationHeader ?? string.Empty);
  }
}
