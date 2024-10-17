using System.Text;
using LGDXRobot2Cloud.API.Repositories;
using LGDXRobot2Cloud.Data.Entities;
using LGDXRobot2Cloud.Utilities.Enums;
using Newtonsoft.Json.Linq;

namespace LGDXRobot2Cloud.API.Services;

public interface ITriggerService
{
  Task<FlowDetail?> GetFlowDetailWithStartTriggerAsync(int flowId, int order);
  Task<FlowDetail?> GetFlowDetailWithEndTriggerAsync(int flowId, int order);
  Task<bool> TriggerApiAsync(Trigger trigger, AutoTask task);
}

public class TriggerService(HttpClient httpClient,
  IFlowDetailRepository flowDetailRepository) : ITriggerService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly IFlowDetailRepository _flowDetailRepository = flowDetailRepository;

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
      (int)TriggerPresetValue.RobotAddress => $"\"{task.AssignedRobot?.Address}\"",
      (int)TriggerPresetValue.RobotNamespace => $"\"{task.AssignedRobot?.Namespace}\"",
      _ => string.Empty,
    };
  }

  private static string GenerateBody(string body, AutoTask task)
  {
    StringBuilder s = new();
    int i = 0, j = 0;
    while (j < body.Length - 1)
    {
      while (j < body.Length && s[j] != '(')
        j++;
      if ((j + 1) < body.Length && body[j + 1] == '(')
      {
        // Start of preset value
        while (i < j)
          s.Append(body[i++]);
        while (j < body.Length && s[j] != ')')
          j++;
        if ((j + 1) < body.Length && body[j + 1] == ')')
        {
          // end of preset value
          // ((xxx))
          // i    j
          var presetValueString = body.Substring(i + 2, j - i - 2);
          if (int.TryParse(presetValueString, out int presetValue))
          {
            var insert = GeneratePresetValue(presetValue, task);
            if (insert != string.Empty)
            {
              // Valid preset value
              s.Append(insert);
              j += 2;
              i = j;
            }
          }
        }
      }
    }
    // Copy remaining content
    while (i < body.Length)
      s.Append(body[i++]);
    return s.ToString();
  }

  public Task<FlowDetail?> GetFlowDetailWithStartTriggerAsync(int flowId, int order)
  {
    return _flowDetailRepository.GetFlowDetailAsync(flowId, order, true);
  }

  public Task<FlowDetail?> GetFlowDetailWithEndTriggerAsync(int flowId, int order)
  {
    return _flowDetailRepository.GetFlowDetailAsync(flowId, order, false);
  }

  public async Task<bool> TriggerApiAsync(Trigger trigger, AutoTask task)
  {
    bool result = true;
    var body = GenerateBody(trigger.Body ?? string.Empty, task);
    Console.WriteLine(body);
    var bodyJson = JObject.Parse(body);
    
    // API Key
    if (trigger.ApiKeyId != null)
    {
      switch (trigger.ApiKeyInsertLocationId)
      {
        case (int) ApiKeyInsertLocation.Header:
          _httpClient.DefaultRequestHeaders.Add(trigger.ApiKeyFieldName ?? "A", trigger.ApiKey?.Secret);
          break;
        case (int) ApiKeyInsertLocation.Body:
          bodyJson.Add(trigger.ApiKeyFieldName ?? "A", trigger.ApiKey?.Secret);
          break;
      }
    }

    var requestBody = new StringContent(bodyJson.ToString(), Encoding.UTF8, "application/json");
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