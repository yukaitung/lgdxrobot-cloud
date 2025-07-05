using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.Utilities.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.ActivityLogs;

public sealed partial class ActivityLogsDetail
{
  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public int? Id { get; set; }

  [SupplyParameterFromQuery]
  private string? ReturnUrl { get; set; }

  ActivityLogDto? ActivityLog { get; set; } = null!;
  TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

  private static string DisplayUser(LgdxUserSearchDto? user)
  {
    if (user == null)
      return "System";
    if (user.UserName != null)
      return user.UserName;
    return "Deleted User";
  }

  private static string DisplaEntityId(ActivityLogDto activityLogDto)
  {
    if (activityLogDto.EntityName!.Equals("Email", StringComparison.OrdinalIgnoreCase))
    {
      return ((EmailType)int.Parse(activityLogDto.EntityId!)).ToEnumMember() ?? "";
    }
    else
    {
      return activityLogDto.EntityId ?? "";
    }
  }


  private static string DisplayApiKey(ApiKeySearchDto? apiKey)
  {
    if (apiKey == null)
      return "Internal / UI";
    if (apiKey.Name != null)
      return apiKey.Name;
    return "Deleted API Key";
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