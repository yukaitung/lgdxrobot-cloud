using System.Text.Json;
using LGDXRobot2Cloud.UI.Client;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.ViewModels.Automation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.Triggers;

public sealed partial class TriggerDetail : ComponentBase, IDisposable
{
  

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required LgdxApiClient LgdxApiClient { get; set; }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<TriggerDetail> ObjectReference = null!;
  private TriggerDetailViewModel TriggerDetailViewModel { get; set; } = new();
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  private readonly string SelectId = $"{nameof(TriggerDetailViewModel.ApiKeyId)}";

 
  // Form
  public void HandleHttpMethod(object args)
  {
    TriggerDetailViewModel.HttpMethodId = int.Parse(args.ToString() ?? string.Empty);
  }

  public void HandleApiKeyInsertAt(object args)
  {
    TriggerDetailViewModel.ApiKeyInsertLocationId = int.Parse(args.ToString() ?? string.Empty);
  }

  [JSInvokable("HandlSelectSearch")]
  public async Task HandlSelectSearch(string elementId, string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    if (elementId == SelectId)
    {
      var response = await LgdxApiClient.Administration.ApiKeys.Search.GetAsync(x => x.QueryParameters = new() {
        Name = name
      });
      string result = JsonSerializer.Serialize(response);
      await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", SelectId, result);
    }
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, string? id, string? name)
  {
    if (elementId == SelectId)
    {
      TriggerDetailViewModel.ApiKeyId = id != null ? int.Parse(id) : null;
      TriggerDetailViewModel.ApiKeyName = name;
    }
  }

  public async Task HandleValidSubmit()
  {
    if (Id != null)
    {
      // Update
      await LgdxApiClient.Automation.Triggers[(int)Id].PutAsync(TriggerDetailViewModel.ToUpdateDto());
    }
    else
    {
      // Create
      await LgdxApiClient.Automation.Triggers.PostAsync(TriggerDetailViewModel.ToCreateDto());
    }
    NavigationManager.NavigateTo(AppRoutes.Automation.Triggers.Index);
  }

  public async Task HandleDelete()
  {
    await LgdxApiClient.Automation.Triggers[(int)Id!].DeleteAsync();
    NavigationManager.NavigateTo(AppRoutes.Automation.Triggers.Index);
  }

  public void BodyAddStep()
  {
    TriggerDetailViewModel.BodyDataList.Add(new BodyData());
  }

  public void BodyRemoveStep(int i)
  {
    if (TriggerDetailViewModel.BodyDataList.Count <= 1)
      return;
    TriggerDetailViewModel.BodyDataList.RemoveAt(i);
  }

  public void HandleBodyPresetChange(int i, object? args)
  {
    if (args != null)
      TriggerDetailViewModel.BodyDataList[i].Value = int.Parse(args.ToString()!);
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var trigger = await LgdxApiClient.Automation.Triggers[(int)_id].GetAsync();
        TriggerDetailViewModel.FromDto(trigger!);
        _editContext = new EditContext(TriggerDetailViewModel);
        _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
      }
      else
      {
        _editContext = new EditContext(TriggerDetailViewModel);
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
    if (TriggerDetailViewModel.ApiKeyRequired)
    {
      await JSRuntime.InvokeVoidAsync("InitAdvancedSelect", SelectId);
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    ObjectReference?.Dispose();
  }
}