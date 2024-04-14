using AutoMapper;
using LGDXRobot2Cloud.Shared.Entities;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Nodes
{
  public partial class NodeDetail
  {
    [Inject]
    public required INodeService NodeService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public int? Id { get; set; }

    private Node _node { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool _isInvalid { get; set; } = false;
    private bool _isError { get; set; } = false;
    private bool _isDeleteError { get; set; } = false;

    private async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await NodeService.UpdateNodeAsync((int)Id, Mapper.Map<NodeCreateDto>(_node));
        if (success)
          await JSRuntime.InvokeVoidAsync("CloseModal", "nodeDetailModal");
        else
          _isError = true;
      }
      else
      {
        // Create
        var success = await NodeService.AddNodeAsync(Mapper.Map<NodeCreateDto>(_node));
        if (success != null)
          await JSRuntime.InvokeVoidAsync("CloseModal", "nodeDetailModal");
        else
          _isError = true;
      }
    }

    private void HandleInvalidSubmit()
    {
      _isInvalid = true;
    }

    private async void HandleDelete()
    {
      if (Id != null)
      {
        var success = await NodeService.DeleteNodeAsync((int)Id);
        if (success)
          await JSRuntime.InvokeVoidAsync("CloseModal", "nodeDeleteModal");
        else
          _isDeleteError = true;
      }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      parameters.SetParameterProperties(this);
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        _isInvalid = false;
        _isError = false;
        _isDeleteError = false;
        if (_id != null)
        {
          var node = await NodeService.GetNodeAsync((int)_id);
          if (node != null) {
            _node = node;
            _editContext = new EditContext(_node);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          }
        }
        else
        {
          _node = new Node();
          _editContext = new EditContext(_node);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }
  }
}