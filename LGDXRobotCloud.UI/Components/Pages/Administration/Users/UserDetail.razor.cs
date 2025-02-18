using System.Text.Json;
using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.Users;

public sealed partial class UserDetail : ComponentBase, IDisposable
{
  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public string? Id { get; set; } = null;

  private DotNetObjectReference<UserDetail> ObjectReference = null!;
  private UserDetailViewModel UserDetailViewModel { get; set; } = new UserDetailViewModel();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  // Form helping variables
  private readonly string[] AdvanceSelectElements = ["RoleId-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;

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
      UserDetailViewModel.Roles[order] = name;
    }
  }

  public void ListAddRole()
  {
    UserDetailViewModel.Roles.Add(string.Empty);
  }

  public async Task ListRemoveRole(int i)
  {
    if (UserDetailViewModel.Roles.Count <= 1)
      return;
    if (i < UserDetailViewModel.Roles.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1, true);
    UserDetailViewModel.Roles.RemoveAt(i);
    InitaisedAdvanceSelect--;
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Administration.Users[UserDetailViewModel.Id].PutAsync(UserDetailViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Administration.Users.PostAsync(UserDetailViewModel.ToCreateDto());
      }
      NavigationManager.NavigateTo(AppRoutes.Administration.Users.Index);
    }
    catch (ApiException ex)
    {
      UserDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Administration.Users[UserDetailViewModel.Id].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Administration.Users.Index);
    }
    catch (ApiException ex)
    {
      UserDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id))
    {
      if (Guid.TryParse(_id, out Guid _guid))
      {
        var user = await LgdxApiClient.Administration.Users[_guid].GetAsync();
        UserDetailViewModel.FromDto(user!);
        _editContext = new EditContext(UserDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        UserDetailViewModel.Roles.Add(string.Empty);
        _editContext = new EditContext(UserDetailViewModel);
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
    if (InitaisedAdvanceSelect < UserDetailViewModel.Roles.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        UserDetailViewModel.Roles.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = UserDetailViewModel.Roles.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}