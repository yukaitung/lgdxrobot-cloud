using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.Blazor;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.NodesCollections
{
  public partial class NodeCollectionDetail : AbstractForm, IDisposable
  {
    [Inject]
    public required INodesCollectionService NodesCollectionService { get; set; }

    [Inject]
    public required INodeService NodeService { get; set; }

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    [Inject]
    public required IMapper Mapper { get; set; }

    [Parameter]
    public int? Id { get; set; }

    [Parameter]
    public EventCallback<(int, string, CrudOperation)> OnSubmitDone { get; set; }

    private DotNetObjectReference<NodeCollectionDetail> ObjectReference = null!;
    private NodesCollectionBlazor NodesCollection { get; set; } = null!;
    private EditContext _editContext = null!;
    private readonly CustomFieldClassProvider _customFieldClassProvider = new();
    private bool IsInvalid { get; set; } = false;
    private bool IsError { get; set; } = false;

    // Form helping variables
    private readonly string[] AdvanceSelectElements = ["NodesId-"];
    private bool UpdateAdvanceSelect { get; set; } = false;
    private bool UpdateAdvanceSelectList { get; set; } = false;

    [JSInvokable("HandlSelectSearch")]
    public async Task HandlSelectSearch(string elementId, string name)
    {
      if (string.IsNullOrWhiteSpace(name))
        return;
      var index = elementId.IndexOf('-');
      if (index == -1 || index + 1 == elementId.Length)
        return;
      string element = elementId[..(index + 1)];
      if (element == AdvanceSelectElements[0])
      {
        var result = await NodeService.SearchNodesAsync(name);
        await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", elementId, result);
      }
    }

    [JSInvokable("HandleSelectChange")]
    public void HandleSelectChange(string elementId, int? id, string? name)
    {
      if (string.IsNullOrWhiteSpace(name))
        return;
      var index = elementId.IndexOf('-');
      if (index == -1 || index + 1 == elementId.Length)
        return;
      string element = elementId[..(index + 1)];
      int order = int.Parse(elementId[(index + 1)..]);
      if (element == AdvanceSelectElements[0])
      {
        NodesCollection.Nodes[order].NodeId = id;
        NodesCollection.Nodes[order].NodeName = name;
      }
    }

    private void TaskAddStep()
    {
      NodesCollection.Nodes.Add(new NodesCollectionDetailBlazor());
      UpdateAdvanceSelect = true;
    }

    private async Task TaskStepMoveUp(int i)
    {
      if (i < 1)
        return;
      (NodesCollection.Nodes[i], NodesCollection.Nodes[i - 1]) = (NodesCollection.Nodes[i - 1], NodesCollection.Nodes[i]);
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i - 1, i);
    }

    private async Task TaskStepMoveDown(int i)
    {
      if (i > NodesCollection.Nodes.Count - 1)
        return;
      (NodesCollection.Nodes[i], NodesCollection.Nodes[i + 1]) = (NodesCollection.Nodes[i + 1], NodesCollection.Nodes[i]);
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1);
    }

    private async Task TaskRemoveStep(int i)
    {
      if (NodesCollection.Nodes.Count <= 1)
        return;
      if (i < NodesCollection.Nodes.Count - 1)
        await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1, true);
      NodesCollection.Nodes.RemoveAt(i);
    }

    protected override async Task HandleValidSubmit()
    {
      if (Id != null)
      {
        // Update
        bool success = await NodesCollectionService.UpdateNodesCollectionAsync((int)Id, Mapper.Map<NodesCollectionUpdateDto>(NodesCollection));
        if (success)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "nodesCollectionDetailModal");
          await OnSubmitDone.InvokeAsync(((int)Id, NodesCollection.Name, CrudOperation.Update));
        }
        else
          IsError = true;
      }
      else
      {
        // Create
        var success = await NodesCollectionService.AddNodesCollectionAsync(Mapper.Map<NodesCollectionCreateDto>(NodesCollection));
        if (success != null)
        {
          await JSRuntime.InvokeVoidAsync("CloseModal", "nodesCollectionDetailModal");
          await OnSubmitDone.InvokeAsync((success.Id, success.Name, CrudOperation.Create));
        }
        else
          IsError = true;
      }
    }

    protected override void HandleInvalidSubmit()
    {
      IsInvalid = true;
    }

    protected override async void HandleDelete()
    {
      if (Id != null)
      {
        var success = await NodesCollectionService.DeleteNodesCollectionAsync((int)Id);
        if (success)
        {
          // DO NOT REVERSE THE ORDER
          await JSRuntime.InvokeVoidAsync("CloseModal", "nodesCollectionDeleteModal");
          await OnSubmitDone.InvokeAsync(((int)Id, NodesCollection.Name, CrudOperation.Delete));
        } 
        else
          IsError = true;
      }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
      parameters.SetParameterProperties(this);
      if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
      {
        IsInvalid = false;
        IsError = false;
        if (_id != null)
        {
          var nodesCollection = await NodesCollectionService.GetNodesCollectionAsync((int)_id);
          if (nodesCollection != null) {
            NodesCollection = nodesCollection;
            for (int i = 0; i < NodesCollection.Nodes.Count; i++)
            {
              NodesCollection.Nodes[i].NodeName = NodesCollection.Nodes[i].Node?.Name;
              NodesCollection.Nodes[i].NodeId = NodesCollection.Nodes[i].Node?.Id;
            }
            _editContext = new EditContext(NodesCollection);
            _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
            UpdateAdvanceSelectList = true;
          }
        }
        else
        {
          NodesCollection = new NodesCollectionBlazor();
          NodesCollection.Nodes.Add(new NodesCollectionDetailBlazor());
          _editContext = new EditContext(NodesCollection);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
          UpdateAdvanceSelect = true;
        }
      }
      await base.SetParametersAsync(ParameterView.Empty);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      await base.OnAfterRenderAsync(firstRender);
      if (firstRender)
      {
        ObjectReference = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("InitDotNet", ObjectReference);
      }
      if (UpdateAdvanceSelect)
      {
        var index = (NodesCollection.Nodes.Count - 1).ToString();
        await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", AdvanceSelectElements, index, 1);
        UpdateAdvanceSelect = false;
      }
      else if (UpdateAdvanceSelectList)
      {
        var index = NodesCollection.Nodes.Count.ToString();
        await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", AdvanceSelectElements, 0, index);
        UpdateAdvanceSelectList = false;
      }
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      ObjectReference?.Dispose();
    }
  }
}