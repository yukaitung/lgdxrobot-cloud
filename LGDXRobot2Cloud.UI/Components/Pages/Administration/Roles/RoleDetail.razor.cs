using AutoMapper;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.ViewModels.Administration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.Roles;

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
    RolesDetailViewModel.Scopes.Add(string.Empty);
  }

  public void ListRemoveScope(int i)
  {
    if (RolesDetailViewModel.Scopes.Count <= 0)
      return;
    RolesDetailViewModel.Scopes.RemoveAt(i);
  }

  public async Task HandleValidSubmit()
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

  public async Task HandleDelete()
  {
    await LgdxApiClient.Administration.Roles[RolesDetailViewModel.Id].DeleteAsync();
    NavigationManager.NavigateTo(AppRoutes.Administration.Roles.Index);
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
      RolesDetailViewModel.Scopes.Add(string.Empty);
      _editContext = new EditContext(RolesDetailViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}