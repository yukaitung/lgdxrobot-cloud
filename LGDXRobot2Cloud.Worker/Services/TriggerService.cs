using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Contracts;
using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.Worker.Services;

public interface ITriggerService
{
  Task<bool> TriggerAsync(AutoTaskTriggerContract autoTaskTriggerContract);
}

public class TriggerService(HttpClient httpClient) : ITriggerService
{
  private readonly HttpClient _httpClient = httpClient;

  private static string GeneratePresetValue(int i, AutoTaskTriggerContract autoTaskTriggerContract)
  {
    return i switch
    {
      (int)TriggerPresetValue.AutoTaskId => $"{autoTaskTriggerContract.AutoTaskId}",
      (int)TriggerPresetValue.AutoTaskName => $"\"{autoTaskTriggerContract.AutoTaskName}\"",
      (int)TriggerPresetValue.AutoTaskCurrentProgressId => $"{autoTaskTriggerContract.AutoTaskCurrentProgressId}",
      (int)TriggerPresetValue.AutoTaskCurrentProgressName => $"\"{autoTaskTriggerContract.AutoTaskCurrentProgressName}\"",
      (int)TriggerPresetValue.RobotId => $"\"{autoTaskTriggerContract.RobotId}\"",
      (int)TriggerPresetValue.RobotName => $"\"{autoTaskTriggerContract.RobotName}\"",
      (int)TriggerPresetValue.RealmId => $"\"{autoTaskTriggerContract.RealmId}\"",
      (int)TriggerPresetValue.RealmName => $"\"{autoTaskTriggerContract.RealmName}\"",
      _ => string.Empty,
    };
  }

  public async Task<bool> TriggerAsync(AutoTaskTriggerContract autoTaskTriggerContract)
  {
    var trigger = autoTaskTriggerContract.Trigger;
    var bodyDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(trigger.Body ?? "{}");
    if (bodyDictionary != null)
    {
      // Replace Preset Value
      foreach (var pair in bodyDictionary)
      {
        if (pair.Value.Length >= 5) // ((1)) has 5 characters
        {
          if (int.TryParse(pair.Value[2..^2], out int p))
          {
            bodyDictionary[pair.Key] = GeneratePresetValue(p, autoTaskTriggerContract);
          }
        }
      }
      // Add Next Token
      if (autoTaskTriggerContract.AutoTaskNextControllerId != (int) AutoTaskNextController.Robot && autoTaskTriggerContract.NextToken != null)
      {
        bodyDictionary.Add("Lgdx2NextToken", autoTaskTriggerContract.NextToken);
      }
      // Add API Key
      if (trigger.ApiKeyId != null)
      {
        switch (trigger.ApiKeyInsertLocationId)
        {
          case (int) ApiKeyInsertLocation.Header:
            _httpClient.DefaultRequestHeaders.Add(trigger.ApiKeyFieldName ?? "Key", trigger.ApiKey?.Secret);
            break;
          case (int) ApiKeyInsertLocation.Body:
            bodyDictionary.Add(trigger.ApiKeyFieldName ?? "Key", trigger.ApiKey?.Secret ?? string.Empty);
            break;
        }
      }
    }

    var bodyStr = bodyDictionary != null ? JsonSerializer.Serialize(bodyDictionary) : string.Empty;
    var requestBody = new StringContent(bodyStr, Encoding.UTF8, "application/json");
    HttpResponseMessage? httpResult = null;
    bool result = true;
    switch (trigger.HttpMethodId)
    {
      case (int) TriggerHttpMethod.Get:
        httpResult = await _httpClient.GetAsync(trigger.Url);
        break;
      case (int) TriggerHttpMethod.Post:
        httpResult = await _httpClient.PostAsync(trigger.Url, requestBody);
        break;
      case (int) TriggerHttpMethod.Put:
        httpResult = await _httpClient.PutAsync(trigger.Url, requestBody);
        break;
      case (int) TriggerHttpMethod.Patch:
        httpResult = await _httpClient.PatchAsync(trigger.Url, requestBody);
        break;
      case (int) TriggerHttpMethod.Delete:
        httpResult = await _httpClient.DeleteAsync(trigger.Url);
        break;
      default:
        result = false;
        break;
    }
    if (httpResult != null && !httpResult.IsSuccessStatusCode) 
    {
      // TODO: Log & Retry
      result = false;
    }

    return result || trigger.SkipOnFailure;
  }
}