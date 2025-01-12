using System.Text.Json;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace LGDXRobot2Cloud.UI.Helpers;

public static class HeaderHelper
{
  public static HeadersInspectionHandlerOption GenrateHeadersInspectionHandlerOption()
  {
    return new HeadersInspectionHandlerOption()
    {
      InspectResponseHeaders = true
    };
  }

  public static PaginationHelper? GetPaginationHelper(HeadersInspectionHandlerOption headersInspectionHandlerOption)
  {
    var paginationHeader = headersInspectionHandlerOption.ResponseHeaders["X-Pagination"].FirstOrDefault();
    return JsonSerializer.Deserialize<PaginationHelper>(paginationHeader ?? string.Empty);
  }
}