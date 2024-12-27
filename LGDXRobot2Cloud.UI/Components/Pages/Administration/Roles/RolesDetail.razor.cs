using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Administration.Roles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Administration.Roles;

public sealed partial class RolesDetail : ComponentBase
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IRolesService RolesService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public string? Id { get; set; } = null;

  private RolesDetailViewModel RolesDetailViewModel { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  public void ListAddScope()
  {
    RolesDetailViewModel.Scopes.Add(string.Empty);
  }

  public void ListRemoveScope(int i)
  {
    if (RolesDetailViewModel.Scopes.Count <= 1)
      return;
    RolesDetailViewModel.Scopes.RemoveAt(i);
  }

  public async Task HandleValidSubmit()
  {
    ApiResponse<bool> response;
    if (Id != null)
    {
      // Update
      response = await RolesService.UpdateRoleAsync(Id, Mapper.Map<LgdxRoleUpdateDto>(RolesDetailViewModel));
    }
    else
    {
      // Create
      response = await RolesService.AddRoleAsync(Mapper.Map<LgdxRoleCreateDto>(RolesDetailViewModel));
    }
    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Setting.Roles.Index);
    else
      RolesDetailViewModel.Errors = response.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await RolesService.DeleteRoleAsync(Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Setting.Roles.Index);
      else
        RolesDetailViewModel.Errors = response.Errors;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id) && _id != null)
    {
      var response = await RolesService.GetRoleAsync(_id);
      var user = response.Data;
      if (user != null) 
      {
        RolesDetailViewModel = Mapper.Map<RolesDetailViewModel>(user);
        _editContext = new EditContext(RolesDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    else
    {
      RolesDetailViewModel = new RolesDetailViewModel();
      RolesDetailViewModel.Scopes.Add(string.Empty);
      _editContext = new EditContext(RolesDetailViewModel);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}