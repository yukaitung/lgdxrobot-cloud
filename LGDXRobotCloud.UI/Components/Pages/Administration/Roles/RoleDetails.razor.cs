using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.Roles;

public partial class RoleDetails : ComponentBase
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public string? Id { get; set; } = null;

  private RolesDetailsViewModel RolesDetailsViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public void ListAddScope()
  {
    RolesDetailsViewModel.Scopes.Add(new ScopeOption());
  }

  public void ListRemoveScope(int i)
  {
    if (RolesDetailsViewModel.Scopes.Count <= 0)
      return;
    RolesDetailsViewModel.Scopes.RemoveAt(i);
  }

  public void HandleAreaChanged(int row, int value)
  {
    if (row < 0 || row >= RolesDetailsViewModel.Scopes.Count)
      return;
    RolesDetailsViewModel.Scopes[row].Area = value;
    RolesDetailsViewModel.Scopes[row].Controller = null;
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Administration.Roles[RolesDetailsViewModel.Id].PutAsync(RolesDetailsViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Administration.Roles.PostAsync(RolesDetailsViewModel.ToCreateDto());
      }
      NavigationManager.NavigateTo(AppRoutes.Administration.Roles.Index);
    }
    catch (ApiException ex)
    {
      RolesDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Administration.Roles[RolesDetailsViewModel.Id].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Administration.Roles.Index);
    }
    catch (ApiException ex)
    {
      RolesDetailsViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id) && _id != null)
    {
      if (Guid.TryParse(_id, out Guid _guid))
      {
        var role = await LgdxApiClient.Administration.Roles[_guid].GetAsync();
        RolesDetailsViewModel.FromDto(role!);
        _editContext = new EditContext(RolesDetailsViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    else
    {
      RolesDetailsViewModel.Scopes.Add(new ScopeOption());
      _editContext = new EditContext(RolesDetailsViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}