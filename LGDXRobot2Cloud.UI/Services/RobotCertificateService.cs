using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotCertificateService
{
  Task<ApiResponse<(IEnumerable<RobotCertificateListDto>?, PaginationHelper?)>> GetRobotCertificatesAsync(int pageNumber = 1, int pageSize = 10);
  Task<ApiResponse<RobotCertificateDto>> GetRobotCertificateAsync(string certificateId);
  Task<ApiResponse<RobotCertificateIssueDto>> RenewRobotCertificateAsync(string certificateId, RobotCertificateRenewRequestDto robotCertificateRenewRequestDto);
  Task<ApiResponse<RootCertificateDto>> GetRootCertificateAsync();
}

public sealed class RobotCertificateService(
    AuthenticationStateProvider authenticationStateProvider, 
    HttpClient httpClient,
    ITokenService tokenService
  ) : BaseService(authenticationStateProvider, httpClient, tokenService), IRobotCertificateService
{
  public async Task<ApiResponse<(IEnumerable<RobotCertificateListDto>?, PaginationHelper?)>>  GetRobotCertificatesAsync(int pageNumber = 1, int pageSize = 10)
  {
    try
    {
      var url = $"Administration/RobotCertificates?pageNumber={pageNumber}&pageSize={pageSize}";
      var response = await _httpClient.GetAsync(url);
      if (response.IsSuccessStatusCode)
      {
        var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
        var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
        var robotCertificates = await JsonSerializer.DeserializeAsync<IEnumerable<RobotCertificateListDto>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<(IEnumerable<RobotCertificateListDto>?, PaginationHelper?)> {
          Data = (robotCertificates, PaginationHelper),
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<RobotCertificateDto>> GetRobotCertificateAsync(string certificateId)
  {
    try
    {
      var response = await _httpClient.GetAsync($"Administration/RobotCertificates/{certificateId}");
      if (response.IsSuccessStatusCode)
      {
        var robotCertificate = await JsonSerializer.DeserializeAsync<RobotCertificateDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<RobotCertificateDto> {
          Data = robotCertificate,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<RobotCertificateIssueDto>> RenewRobotCertificateAsync(string certificateId, RobotCertificateRenewRequestDto robotCertificateRenewRequestDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(robotCertificateRenewRequestDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync($"Administration/RobotCertificates/{certificateId}/Renew", content);
      var robotCertificate = await JsonSerializer.DeserializeAsync<RobotCertificateIssueDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<RobotCertificateIssueDto> {
          Data = robotCertificate,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }

  public async Task<ApiResponse<RootCertificateDto>> GetRootCertificateAsync()
  {
    try
    {
      var response = await _httpClient.GetAsync("Administration/RobotCertificates/Root");
      if (response.IsSuccessStatusCode)
      {
        var rootCertificate = await JsonSerializer.DeserializeAsync<RootCertificateDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<RootCertificateDto> {
          Data = rootCertificate,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else
      {
        throw new Exception($"{ApiHelper.UnexpectedResponseStatusCodeMessage}{response.StatusCode}");
      }
    }
    catch (Exception ex)
    {
      throw new Exception(ApiHelper.ApiErrorMessage, ex);
    }
  }
}