using AutoMapper;
using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Nodes
{
  public partial class NodeUpdate
  {
    [Inject]
    public required INodeService NodeService { get; set; }
    
    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public int Id { get; set; }

    private NodeDto _node { get; set; } = null!;
    private EditContext _updateContext = null!;

    // Form
    private bool _isInvalid { get; set; } = false;
    private bool _isError { get; set; } = false;
 
    private async Task HandleValidSubmit()
    {
      _isInvalid = false;
      bool success = await NodeService.UpdateNodeAsync(Id, Mapper.Map<NodeCreateDto>(_node));
      if(success)
      {
        _isError = false;
        await JSRuntime.InvokeVoidAsync("CloseModal", "nodeUpdateModal");
      }
      else
        _isError = true;
    }

    private void HandleInvalidSubmit()
    {
      _isInvalid = true;
    }

    protected override void OnInitialized()
    {
      _node = new NodeDto();
      _updateContext = new EditContext(_node);
      _updateContext.SetFieldCssClassProvider(new CustomFieldClassProvider());
    }

    public override async Task SetParametersAsync(ParameterView parameters) 
    {
      parameters.SetParameterProperties(this);
      if (parameters.TryGetValue<int>(nameof(Id), out var _id))
      {
        if (_id != 0)
        {
          _isError = false;
          var node = await NodeService.GetNodeAsync(_id);
          if (node != null)
            _node = node;
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }
  }
}