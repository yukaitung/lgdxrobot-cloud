using System.Text;
using System.Text.Json;
using AutoMapper;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.UI.Constants;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.UI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LGDXRobot2Cloud.UI.Components.Pages.Navigation.Triggers;

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
  private Trigger Trigger { get; set; } = null!;
  private List<BodyData> Body { get; set; } = [];
  private EditContext _editContext = null!;
  private readonly CustomFieldClassProvider _customFieldClassProvider = new();
  private bool IsError { get; set; } = false;

  private readonly string SelectId = "ApiKeyId";

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
    Trigger.HttpMethodId = int.Parse(args.ToString() ?? string.Empty);
  }

  public void HandleApiKeyInsertAt(object args)
  {
    Trigger.ApiKeyInsertLocationId = int.Parse(args.ToString() ?? string.Empty);
  }

  [JSInvokable("HandlSelectSearch")]
  public async Task HandlSelectSearch(string elementId, string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      return;
    if (elementId == SelectId)
    {
      var result = await ApiKeyService.SearchApiKeysAsync(name);
      await JSRuntime.InvokeVoidAsync("AdvanceSelectUpdate", SelectId, result);
    }
  }

  [JSInvokable("HandleSelectChange")]
  public void HandleSelectChange(string elementId, int? id, string? name)
  {
    if (elementId == SelectId)
    {
      Trigger.ApiKeyId = id;
      Trigger.ApiKeyName = name;
    }
  }

  public async Task HandleValidSubmit()
  {
    bool success;
    Trigger.Body = GenerateBodyJson();

    if (Id != null)
      // Update
      success = await TriggerService.UpdateTriggerAsync((int)Id, Mapper.Map<TriggerUpdateDto>(Trigger));
    else
      // Create
      success = await TriggerService.AddTriggerAsync(Mapper.Map<TriggerCreateDto>(Trigger));
    
    if (success)
      NavigationManager.NavigateTo(AppRoutes.Navigation.Triggers.Index);
    else 
      IsError = true;
  }

  public async Task HandleDelete()
  {
    if (Id != null)
    {
      var success = await TriggerService.DeleteTriggerAsync((int)Id);
      if (success)
        NavigationManager.NavigateTo(AppRoutes.Navigation.Triggers.Index);
      else
        IsError = true;
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
        var trigger = await TriggerService.GetTriggerAsync((int)_id);
        if (trigger != null)
        {
          Trigger = trigger;
          if (trigger.ApiKey != null)
          {
            Trigger.ApiKeyRequired = true;
            Trigger.ApiKeyId = trigger.ApiKey.Id;
            Trigger.ApiKeyName = trigger.ApiKey.Name;
          }
          ConvertBodyJson(trigger.Body?.ToString() ?? string.Empty);
          _editContext = new EditContext(Trigger);
          _editContext.SetFieldCssClassProvider(_customFieldClassProvider);
        }
      }
      else
      {
        Trigger = new Trigger();
        _editContext = new EditContext(Trigger);
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
    if (Trigger.ApiKeyRequired)
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