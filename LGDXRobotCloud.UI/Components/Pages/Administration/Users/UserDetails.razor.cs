using System.Text.Json;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.Services;
using LGDXRobotCloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.Users;

public partial class UserDetails : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required ITokenService TokenService { get; set; }

  [Inject]
  public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }

  [Parameter]
  public string? Id { get; set; } = null;

  private DotNetObjectReference<UserDetails> ObjectReference = null!;
  private UserDetailsViewModel UserDetailsViewModel { get; set; } = new UserDetailsViewModel();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form helping variables
  private readonly string[] AdvanceSelectElements = ["RoleId-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;
  TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

  [JSInvokable("HandlSelectSearch")]
  public async Task HandlSelectSearch(string elementId, string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    var index = elementId.IndexOf('-');
    if (index == -1 || index + 1 == elementId.Length)
      return;
    string element = elementId[..(index + 1)];
    if (element == AdvanceSelectElements[0])
    {
      var result = await LgdxApiClient.Administration.Roles.Search.GetAsync(x => x.QueryParameters = new(){
        Name = name
      });
      string str = JsonSerializer.Serialize(result);
      await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, str);
    }
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, string? id, string? name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    var index = elementId.IndexOf('-');
    if (index == -1 || index + 1 == elementId.Length)
      return;
    string element = elementId[..(index + 1)];
    int order = int.Parse(elementId[(index + 1)..]);
    if (element == AdvanceSelectElements[0])
    {
      UserDetailsViewModel.Roles[order] = name;
    }
  }

  public void ListAddRole()
  {
    UserDetailsViewModel.Roles.Add(string.Empty);
  }

  public async Task ListRemoveRole(int i)
  {
    if (UserDetailsViewModel.Roles.Count <= 1)
      return;
    if (i < UserDetailsViewModel.Roles.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1, true);
    UserDetailsViewModel.Roles.RemoveAt(i);
    InitaisedAdvanceSelect--;
  }

  public async Task HandleUnlockUser()
  {
    try
    {
      await LgdxApiClient.Administration.Users[UserDetailsViewModel.Id].Unlock.PatchAsync();
      UserDetailsViewModel.LockoutEnd = null;
    }
    catch (ApiException ex)
    {
      UserDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Administration.Users[UserDetailsViewModel.Id].PutAsync(UserDetailsViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Administration.Users.PostAsync(UserDetailsViewModel.ToCreateDto());
      }
      NavigationManager.NavigateTo(AppRoutes.Administration.Users.Index);
    }
    catch (ApiException ex)
    {
      UserDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Administration.Users[UserDetailsViewModel.Id].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Administration.Users.Index);
    }
    catch (ApiException ex)
    {
      UserDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
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
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id))
    {
      if (Guid.TryParse(_id, out Guid _guid))
      {
        var user = await LgdxApiClient.Administration.Users[_guid].GetAsync();
        UserDetailsViewModel.FromDto(user!);
        _editContext = new EditContext(UserDetailsViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        UserDetailsViewModel.Roles.Add(string.Empty);
        _editContext = new EditContext(UserDetailsViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender)
    {
      ObjectReference = DotNetObjectReference.Create(this);
      await JSRuntime.InvokeVoidAsync("InitDotNet", ObjectReference);
    }
    if (InitaisedAdvanceSelect < UserDetailsViewModel.Roles.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        UserDetailsViewModel.Roles.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = UserDetailsViewModel.Roles.Count;
    }
  }

  public void Dispose()
  {
    ObjectReference?.Dispose();
    GC.SuppressFinalize(this);
  }
}