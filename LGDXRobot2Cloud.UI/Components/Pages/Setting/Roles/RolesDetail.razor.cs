using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Setting.Roles;

public sealed partial class RolesDetail : ComponentBase
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IRolesService IRolesService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public string? Id { get; set; } = null;

  private LgdxRole Role { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  public void TaskAddStep()
  {
    Role.Scopes.Add(string.Empty);
  }

  public void TaskRemoveStep(int i)
  {
    if (Role.Scopes.Count <= 1)
      return;
    Role.Scopes.RemoveAt(i);
  }

  public async Task HandleValidSubmit()
  {
    bool success;

    if (Id != null)
    {
      // Update
      var response = await IRolesService.UpdateRoleAsync(Id, Mapper.Map<LgdxRoleUpdateDto>(Role));
      success = response.IsSuccess;
    }
    else
    {
      // Create
      var response = await IRolesService.AddRoleAsync(Mapper.Map<LgdxRoleCreateDto>(Role));
      success = response.IsSuccess;
    }
      
    if (success)
      NavigationManager.NavigateTo(AppRoutes.Setting.Roles.Index);
    else
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await IRolesService.DeleteRoleAsync(Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Setting.Roles.Index);
      else
        IsError = true;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<string?>(nameof(Id), out var _id) && _id != null)
    {
      var response = await IRolesService.GetRoleAsync(_id);
      var user = response.Data;
      if (user != null) 
      {
        Role = user;
        _editContext = new EditContext(Role);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    else
    {
      Role = new LgdxRole();
      Role.Scopes.Add(string.Empty);
      _editContext = new EditContext(Role);
      _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}