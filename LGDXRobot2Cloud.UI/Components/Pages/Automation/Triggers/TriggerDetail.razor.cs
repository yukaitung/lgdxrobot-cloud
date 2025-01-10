using System.Text;
using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Services;
using LGDXRobot2Cloud.UI.ViewModels.Automation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Automation.Triggers;

public sealed partial class TriggerDetail : ComponentBase, IDisposable
{
  private record BodyData
  {
    public string Key { get; set; } = string.Empty;
    public int Value { get; set; } = 0;
    public string CustomValue { get; set; } = string.Empty;
  }

  [Inject]
  public required ITriggerService TriggerService { get; set; }

  [Inject]
  public required IApiKeyService ApiKeyService { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; } = default!;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required IMapper Mapper { get; set; }

  [Parameter]
  public int? Id { get; set; }

  private DotNetObjectReference<TriggerDetail> ObjectReference = null!;
  private TriggerDetailViewModel TriggerDetailViewModel { get; set; } = null!;
  private List<BodyData> Body { get; set; } = [];
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();

  private readonly string SelectId = $"{nameof(TriggerDetailViewModel.ApiKeyId)}";

  private string GenerateBodyJson()
  {
    StringBuilder s = new();
    for (int i = 0; i < Body.Count; i++)
    {
      var row = Body[i];
      if (row.Value == 0)
      {
        s.Append($"\"{row.Key}\":\"{row.CustomValue}\"");
        if (i < Body.Count - 1)
          s.Append(',');
      }
      else
      {
        s.Append($"\"{row.Key}\":\"(({row.Value}))\"");
        if (i < Body.Count - 1)
          s.Append(',');
      }
    }
    s.Insert(0, '{');
    s.Append('}');
    return s.ToString();
  }

  private void ConvertBodyJson(string body)
  {
    try
    {
      var bodyDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
      if (bodyDictionary == null)
      {
        return;
      }
        
      foreach (var pair in bodyDictionary)
      {
        bool isPreset = false;
        int preset = 0;
        if (pair.Value.Length >= 5) // ((1)) has 5 characters
        {
          if (int.TryParse(pair.Value[2..^2], out int p))
          {
            isPreset = true;
            preset = p;
          }
          else
          {
            isPreset = false;
          }
        }

        var row = new BodyData
        {
          Key = pair.Key,
          Value = preset,
          CustomValue = isPreset ? string.Empty : pair.Value
        };
        Body.Add(row);
      }
    }
    catch (Exception)
    {
      return;
    }
  }

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
      var response = await ApiKeyService.SearchApiKeysAsync(name);
      if (response.IsSuccess)
      {
        var result = response.Data;
        await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", SelectId, result);
      }
    }
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, int? id, string? name)
  {
    if (elementId == SelectId)
    {
      TriggerDetailViewModel.ApiKeyId = id;
      TriggerDetailViewModel.ApiKeyName = name;
    }
  }

  public async Task HandleValidSubmit()
  {
    TriggerDetailViewModel.Body = GenerateBodyJson();
    ApiResponse<bool> response;
    if (Id != null)
      // Update
      response = await TriggerService.UpdateTriggerAsync((int)Id, Mapper.Map<TriggerUpdateDto>(TriggerDetailViewModel));
    else
      // Create
      response = await TriggerService.AddTriggerAsync(Mapper.Map<TriggerCreateDto>(TriggerDetailViewModel));
    
    if (response.IsSuccess)
      NavigationManager.NavigateTo(AppRoutes.Automation.Triggers.Index);
    else 
      TriggerDetailViewModel.Errors = response.Errors;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var response = await TriggerService.DeleteTriggerAsync((int)Id);
      if (response.IsSuccess)
        NavigationManager.NavigateTo(AppRoutes.Automation.Triggers.Index);
      else
        TriggerDetailViewModel.Errors = response.Errors;
    }
  }

  public void BodyAddStep()
  {
    Body.Add(new BodyData());
  }

  public void BodyRemoveStep(int i)
  {
    if (Body.Count <= 1)
      return;
    Body.RemoveAt(i);
  }

  public void HandleBodyPresetChange(int i, object? args)
  {
    if (args != null)
      Body[i].Value = int.Parse(args.ToString()!);
  }

  public override async Task SetParametersAsync(ParameterView parameters)
  {
    parameters.SetParameterProperties(this);
    if (parameters.TryGetValue<int?>(nameof(Id), out var _id))
    {
      if (_id != null)
      {
        var response = await TriggerService.GetTriggerAsync((int)_id);
        var trigger = response.Data;
        if (trigger != null)
        {
          TriggerDetailViewModel = Mapper.Map<TriggerDetailViewModel>(trigger);
          if (trigger.ApiKey != null)
          {
            TriggerDetailViewModel.ApiKeyRequired = true;
            TriggerDetailViewModel.ApiKeyId = trigger.ApiKey.Id;
            TriggerDetailViewModel.ApiKeyName = trigger.ApiKey.Name;
          }
          ConvertBodyJson(trigger.Body?.ToString() ?? string.Empty);
          _editContext = new EditContext(TriggerDetailViewModel);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        TriggerDetailViewModel = new TriggerDetailViewModel();
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