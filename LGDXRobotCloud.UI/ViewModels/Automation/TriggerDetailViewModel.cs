using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using LGDXRobotCloud.UI.Client.Models;
using LGDXRobotCloud.UI.ViewModels.Shared;

namespace LGDXRobotCloud.UI.ViewModels.Automation;

public record BodyData
{
  public string Key { get; set; } = string.Empty;
  public int Value { get; set; } = 0;
  public string CustomValue { get; set; } = string.Empty;
}

public class TriggerDetailViewModel : FormViewModel, IValidatableObject
{
  public int Id { get; set; }

  [Required (ErrorMessage = "Please enter a name.")]
  [MaxLength(50)]
  public string Name { get; set; } = null!;

  [Required (ErrorMessage = "Please enter an URL.")]
  [MaxLength(200)]
  public string Url { get; set; } = null!;

  public int HttpMethodId { get; set; } = 1;

  public List<BodyData> BodyDataList { get; set; } = [];

  public string? Body { get; set; }

  // API Keys
  public bool ApiKeyRequired { get; set; } = false;
  
  public int? ApiKeyInsertLocationId { get; set; } = 1;

  [MaxLength(50)]
  public string? ApiKeyFieldName { get; set; }

  public int? ApiKeyId { get; set; }

  public string? ApiKeyName { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (ApiKeyRequired)
    {
      if (ApiKeyFieldName == null)
      {
        yield return new ValidationResult("Please enter a field name.", [nameof(ApiKeyFieldName)]);
      }
      if (ApiKeyId == null)
      {
        yield return new ValidationResult("Please select an API Key.", [nameof(ApiKeyId)]);
      }
    }
  }

  public static string GenerateBodyJson(List<BodyData> bodyDataList)
  {
    StringBuilder s = new();
    for (int i = 0; i < bodyDataList.Count; i++)
    {
      var row = bodyDataList[i];
      if (row.Value == 0)
      {
        s.Append($"\"{row.Key}\":\"{row.CustomValue}\"");
        if (i < bodyDataList.Count - 1)
          s.Append(',');
      }
      else
      {
        s.Append($"\"{row.Key}\":\"(({row.Value}))\"");
        if (i < bodyDataList.Count - 1)
          s.Append(',');
      }
    }
    s.Insert(0, '{');
    s.Append('}');
    return s.ToString();
  }

  public static List<BodyData> GenerateBodyDataList(string body)
  {
    var bodyDataList = new List<BodyData>();
    var bodyDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
    if (bodyDictionary == null)
    {
      return bodyDataList;
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
      bodyDataList.Add(row);
    }
    return bodyDataList;
  }
}

public static class TriggerDetailViewModelExtensions
{
  public static void FromDto(this TriggerDetailViewModel triggerDetailViewModel, TriggerDto triggerDto)
  {
    triggerDetailViewModel.Id = (int)triggerDto.Id!;
    triggerDetailViewModel.Name = triggerDto.Name!;
    triggerDetailViewModel.Url = triggerDto.Url!;
    triggerDetailViewModel.HttpMethodId = (int)triggerDto.HttpMethodId!;
    triggerDetailViewModel.BodyDataList = TriggerDetailViewModel.GenerateBodyDataList(triggerDto.Body?.ToString() ?? string.Empty);
    triggerDetailViewModel.ApiKeyRequired = triggerDto.ApiKey != null;
    triggerDetailViewModel.ApiKeyInsertLocationId = triggerDto.ApiKeyInsertLocationId;
    triggerDetailViewModel.ApiKeyFieldName = triggerDto.ApiKeyFieldName;
    triggerDetailViewModel.ApiKeyId = triggerDto.ApiKey?.Id;
    triggerDetailViewModel.ApiKeyName = triggerDto.ApiKey?.Name;
  }

  public static TriggerUpdateDto ToUpdateDto(this TriggerDetailViewModel triggerDetailViewModel)
  {
    return new TriggerUpdateDto {
      Name = triggerDetailViewModel.Name,
      Url = triggerDetailViewModel.Url,
      HttpMethodId = triggerDetailViewModel.HttpMethodId,
      Body = TriggerDetailViewModel.GenerateBodyJson(triggerDetailViewModel.BodyDataList),
      ApiKeyInsertLocationId = triggerDetailViewModel.ApiKeyRequired ? triggerDetailViewModel.ApiKeyInsertLocationId : null,
      ApiKeyFieldName = triggerDetailViewModel.ApiKeyRequired ? triggerDetailViewModel.ApiKeyFieldName : null,
      ApiKeyId = triggerDetailViewModel.ApiKeyRequired ? triggerDetailViewModel.ApiKeyId : null
    };
  }

  public static TriggerCreateDto ToCreateDto(this TriggerDetailViewModel triggerDetailViewModel)
  {
    return new TriggerCreateDto {
      Name = triggerDetailViewModel.Name,
      Url = triggerDetailViewModel.Url,
      HttpMethodId = triggerDetailViewModel.HttpMethodId,
      Body = TriggerDetailViewModel.GenerateBodyJson(triggerDetailViewModel.BodyDataList),
      ApiKeyInsertLocationId = triggerDetailViewModel.ApiKeyRequired ? triggerDetailViewModel.ApiKeyInsertLocationId : null,
      ApiKeyFieldName = triggerDetailViewModel.ApiKeyRequired ? triggerDetailViewModel.ApiKeyFieldName : null,
      ApiKeyId = triggerDetailViewModel.ApiKeyRequired ? triggerDetailViewModel.ApiKeyId : null
    };
  }
}