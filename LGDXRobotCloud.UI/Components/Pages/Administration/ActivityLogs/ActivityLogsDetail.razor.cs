using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using Microsoft.AspNetCore.Components;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.ActivityLogs;

public sealed partial class ActivityLogsDetail
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public int? Id { get; set; }

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  ActivityLogDto? ActivityLog { get; set; } = null!;

  private static string DisplayUser(LgdxUserSearchDto? user)
  {
    if (user == null)
      return "System";
    if (user.UserName != null)
      return user.UserName;
    return "Deleted User";
  }

  private static string DisplayApiKey(ApiKeySearchDto? apiKey)
  {
    if (apiKey == null)
      return "Internal / UI";
    if (apiKey.Name != null)
      return apiKey.Name;
    return "Deleted API Key";
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        ActivityLog = await LgdxApiClient.Administration.ActivityLogs[_id.Value].GetAsync();
      }
      else
      {
        NavigationManager.NavigateTo(AppRoutes.Administration.ActivityLogs.Index);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}