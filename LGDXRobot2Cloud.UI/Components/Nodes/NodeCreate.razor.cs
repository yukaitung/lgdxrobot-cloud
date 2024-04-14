using LGDXRobot2Cloud.Shared.Models;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Nodes
{
  public partial class NodeCreate
  {
    [Inject]
    public required INodeService NodeService { get; set; }
    
    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    private NodeCreateDto _node { get; set; } = null!;
    private EditContext _editContext = null!;

    private bool _isInvalid { get; set; } = false;
    private bool _isError { get; set; } = false;
 
    private async Task HandleValidSubmit()
    {
      _isInvalid = false;
      var node = await NodeService.AddNodeAsync(_node);
      if(node != null)
      {
        _isError = false;
        _node = new NodeCreateDto();
        await JSRuntime.InvokeVoidAsync("CloseModal", "nodeCreateModal");
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
      _node = new NodeCreateDto();
      _editContext = new EditContext(_node);
      _editContext.SetFieldCssClassProvider(new CustomFieldClassProvider());
    }
  }
}