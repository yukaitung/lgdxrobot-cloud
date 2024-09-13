using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.Nodes;

public partial class NodeDetail
{
  [Inject]
  public NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required INodeService NodeService { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; } = null;

  private Node Node { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  private async Task HandleValidSubmit()
  {
    bool success = false;
    
    if (Id != null)
      // Update
      success = await NodeService.UpdateNodeAsync((int)Id, Mapper.Map<NodeUpdateDto>(Node));
    else
      // Create
      success = await NodeService.AddNodeAsync(Mapper.Map<NodeCreateDto>(Node));
    
    if (success)
      NavigationManager.NavigateTo(AppRoutes.Robot.Nodes.Index);
    else
      IsError = true;
  }

  private void HandleDelete()
  {
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var node = await NodeService.GetNodeAsync((int)_id);
        if (node != null) {
          Node = node;
          _editContext = new EditContext(Node);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        Node = new Node();
        _editContext = new EditContext(Node);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
    }
    await base.SetParametersAsync(ParameterView.Empty);
  }
}