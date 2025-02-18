using LGDXRobotCloud.UI.Client;
using LGDXRobotCloud.UI.Constants;
using LGDXRobotCloud.UI.Helpers;
using LGDXRobotCloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Kiota.Abstractions;

namespace LGDXRobotCloud.UI.Components.Pages.Administration.Roles;

public sealed partial class RoleDetail : ComponentBase
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Parameter]
  public string? Id { get; set; } = null;

  private RolesDetailViewModel RolesDetailViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public void ListAddScope()
  {
    RolesDetailViewModel.Scopes.Add(new ScopeOption());
  }

  public void ListRemoveScope(int i)
  {
    if (RolesDetailViewModel.Scopes.Count <= 0)
      return;
    RolesDetailViewModel.Scopes.RemoveAt(i);
  }

  public void HandleAreaChanged(int row, int value)
  {
    if (row < 0 || row >= RolesDetailViewModel.Scopes.Count)
      return;
    RolesDetailViewModel.Scopes[row].Area = value;
    RolesDetailViewModel.Scopes[row].Controller = null;
  }

  public async Task HandleValidSubmit()
  {
    try
    {
      if (Id != null)
      {
        // Update
        await LgdxApiClient.Administration.Roles[RolesDetailViewModel.Id].PutAsync(RolesDetailViewModel.ToUpdateDto());
      }
      else
      {
        // Create
        await LgdxApiClient.Administration.Roles.PostAsync(RolesDetailViewModel.ToCreateDto());
      }
      NavigationManager.NavigateTo(AppRoutes.Administration.Roles.Index);
    }
    catch (ApiException ex)
    {
      RolesDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
    }
  }

  public async Task HandleDelete()
  {
    try
    {
      await LgdxApiClient.Administration.Roles[RolesDetailViewModel.Id].DeleteAsync();
      NavigationManager.NavigateTo(AppRoutes.Administration.Roles.Index);
    }
    catch (ApiException ex)
    {
      RolesDetailViewModel.Errors = ApiHelper.GenerateErrorDictionary(ex);
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
        RolesDetailViewModel.FromDto(role!);
        _editContext = new EditContext(RolesDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    else
    {
      RolesDetailViewModel.Scopes.Add(new ScopeOption());
      _editContext = new EditContext(RolesDetailViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}