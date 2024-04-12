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

    private NodeCreateDto _nodeCreate { get; set; } = null!;

    private EditContext _editContext = null!;

    private bool _isInvalid { get; set; } = false;

    private bool _isError { get; set; } = false;
 
    private async Task HandleValidCreate()
    {
      _isInvalid = false;
      var node = await NodeService.AddNodeAsync(_nodeCreate);
      if(node != null)
      {
        _isError = false;
        await JSRuntime.InvokeVoidAsync("CloseModal", "nodeDetailModal");
      }
      else
        _isError = true;
    }

    private void HandleInvalidCreate()
    {
      _isInvalid = true;
    }

    protected override void OnInitialized()
    {
      
      _nodeCreate = new NodeCreateDto();
      _editContext = new EditContext(_nodeCreate);
      _editContext.SetFieldCssClassProvider(new CustomFieldClassProvider());
    }
  }
}