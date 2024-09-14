using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Robot.NodesCollections;

public sealed partial class NodeCollectionDetail : ComponentBase, IDisposable
{
  [Inject]
  public required INodesCollectionService NodesCollectionService { get; set; }

  [Inject]
  public required INodeService NodeService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<NodeCollectionDetail> ObjectReference = null!;
  private NodesCollection NodesCollection { get; set; } = null!;
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  // Form helping variables
  private readonly string[] AdvanceSelectElements = ["NodesId-"];
  private int InitaisedAdvanceSelect { get; set; } = 0;

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

  public void TaskAddStep()
  {
    NodesCollection.Nodes.Add(new NodesCollectionDetail());
  }

  public async Task TaskRemoveStep(int i)
  {
    if (NodesCollection.Nodes.Count <= 1)
      return;
    if (i < NodesCollection.Nodes.Count - 1)
      await JSRuntime.InvokeVoidAsync("AdvanceControlExchange", AdvanceSelectElements, i, i + 1, true);
    NodesCollection.Nodes.RemoveAt(i);
  }

  public async Task HandleValidSubmit()
  {
    bool success;
    if (Id != null)
      // Update
      success = await NodesCollectionService.UpdateNodesCollectionAsync((int)Id, Mapper.Map<NodesCollectionUpdateDto>(NodesCollection));
    else
      // Create
      success = await NodesCollectionService.AddNodesCollectionAsync(Mapper.Map<NodesCollectionCreateDto>(NodesCollection));
  
    if (success)
      NavigationManager.NavigateTo(AppRoutes.Robot.NodesCollections.Index);
    else
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await NodesCollectionService.DeleteNodesCollectionAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Robot.NodesCollections.Index);
      else
        IsError = true;
    }
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
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
        }
      }
      else
      {
        NodesCollection = new NodesCollection();
        NodesCollection.Nodes.Add(new NodesCollectionDetail());
        _editContext = new EditContext(NodesCollection);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
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
    if (InitaisedAdvanceSelect < NodesCollection.Nodes.Count)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelectList", 
        AdvanceSelectElements,
        InitaisedAdvanceSelect,
        NodesCollection.Nodes.Count - InitaisedAdvanceSelect);
      InitaisedAdvanceSelect = NodesCollection.Nodes.Count;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}
