using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Enums;

namespace LGDXRobot2Cloud.API.Services;

public interface ITriggerService
{
  Task<bool> InitiateTriggerAsync(AutoTask? task, FlowDetail? flowDetail);
}

public class TriggerService(HttpClient httpClient) : ITriggerService
{
  private readonly HttpClient _httpClient = httpClient;

  private static string GeneratePresetValue(int i, AutoTask task)
  {
    return i switch
    {
      (int)TriggerPresetValue.AutoTaskId => $"{task.Id}",
      (int)TriggerPresetValue.AutoTaskName => $"\"{task.Name}\"",
      (int)TriggerPresetValue.AutoTaskCurrentProgressId => $"{task.CurrentProgressId}",
      (int)TriggerPresetValue.AutoTaskCurrentProgressName => $"\"{task.CurrentProgress.Name}\"",
      (int)TriggerPresetValue.RobotId => $"\"{task.AssignedRobotId}\"",
      (int)TriggerPresetValue.RobotName => $"\"{task.AssignedRobot?.Name}\"",
      _ => string.Empty,
    };
  }

  public async Task<bool> InitiateTriggerAsync(AutoTask? task, FlowDetail? flowDetail)
  {
    if (task == null || flowDetail == null || flowDetail.Trigger == null)
      return true; // Success because no error

    var trigger = flowDetail.Trigger;
    bool result = true;

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
            bodyDictionary[pair.Key] = GeneratePresetValue(p, task);
          }
        }
      }
      // Add Next Token
      if (flowDetail.AutoTaskNextControllerId != (int) AutoTaskNextController.Robot && task.NextToken != null)
      {
        bodyDictionary.Add("Lgdx2NextToken", task.NextToken);
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
    switch (trigger.HttpMethodId)
    {
      case (int) TriggerHttpMethod.Get:
        var httpResult = await _httpClient.GetAsync(trigger.Url);
        if (!httpResult.IsSuccessStatusCode) 
        {
          // TODO: Log & Retry
          result = false;
        }
        break;
      case (int) TriggerHttpMethod.Post:
        var httpResultPost = await _httpClient.PostAsync(trigger.Url, requestBody);
        if (!httpResultPost.IsSuccessStatusCode) 
        {
          // TODO: Log & Retry
          result = false;
        }
        break;
      case (int) TriggerHttpMethod.Put:
        var httpResultPut = await _httpClient.PutAsync(trigger.Url, requestBody);
        if (!httpResultPut.IsSuccessStatusCode) 
        {
          // TODO: Log & Retry
          result = false;
        }
        break;
      case (int) TriggerHttpMethod.Patch:
        var httpResultPatch = await _httpClient.PatchAsync(trigger.Url, requestBody);
        if (!httpResultPatch.IsSuccessStatusCode) 
        {
          // TODO: Log & Retry
          result = false;
        }
        break;
      case (int) TriggerHttpMethod.Delete:
        var httpResultDelete = await _httpClient.DeleteAsync(trigger.Url);
        if (!httpResultDelete.IsSuccessStatusCode) 
        {
          // TODO: Log & Retry
          result = false;
        }
        break;
      default:
        result = false;
        break;
    }

    return result || trigger.SkipOnFailure;
  }
}