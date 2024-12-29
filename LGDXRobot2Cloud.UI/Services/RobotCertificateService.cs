using System.Text;
using System.Text.Json;
using LGDXRobot2Cloud.Data.Models.DTOs.Commands;
using LGDXRobot2Cloud.Data.Models.DTOs.Responses;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Models;
using LGDXRobot2Cloud.Utilities.Helpers;
using Microsoft.AspNetCore.Components.Authorization;

namespace LGDXRobot2Cloud.UI.Services;

public interface IRobotCertificateService
{
  Task<(IEnumerable<RobotCertificate>?, PaginationHelper?)> GetRobotCertificatesAsync(int pageNumber = 1, int pageSize = 10);
  Task<RobotCertificate?> GetRobotCertificateAsync(string certificateId);
  Task<RobotCertificateIssueDto?> RenewRobotCertificateAsync(string certificateId, RobotRenewCertificateRenewDto dto);
  Task<RootCertificateDto?> GetRootCertificateAsync();
}

public sealed class RobotCertificateService(
  AuthenticationStateProvider authenticationStateProvider, 
  HttpClient httpClient) : BaseService(authenticationStateProvider, httpClient), IRobotCertificateService
{
  public async Task<(IEnumerable<RobotCertificate>?, PaginationHelper?)> GetRobotCertificatesAsync(int pageNumber = 1, int pageSize = 10)
  {
    var url = $"setting/certificates?pageNumber={pageNumber}&pageSize={pageSize}";
    var response = await _httpClient.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
      var PaginationHelperJson = response.Headers.GetValues("X-Pagination").FirstOrDefault() ?? string.Empty;
      var PaginationHelper = JsonSerializer.Deserialize<PaginationHelper>(PaginationHelperJson, _jsonSerializerOptions);
      var certificates = await JsonSerializer.DeserializeAsync<IEnumerable<RobotCertificate>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
      return (certificates, PaginationHelper);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<RobotCertificate?> GetRobotCertificateAsync(string certificateId)
  {
    var response = await _httpClient.GetAsync($"setting/certificates/{certificateId}");
    var certificate = await JsonSerializer.DeserializeAsync<RobotCertificate>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return certificate;
  }

  public async Task<RobotCertificateIssueDto?> RenewRobotCertificateAsync(string certificateId, RobotRenewCertificateRenewDto dto)
  {
    var json = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync($"setting/certificates/{certificateId}/renew", json);
    if (response.IsSuccessStatusCode)
    {
      return await JsonSerializer.DeserializeAsync<RobotCertificateIssueDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    }
    else
    {
      throw new Exception($"The API service returns status code {response.StatusCode}.");
    }
  }

  public async Task<RootCertificateDto?> GetRootCertificateAsync()
  {
    var response = await _httpClient.GetAsync("setting/certificates/root");
    var rootCertificate = await JsonSerializer.DeserializeAsync<RootCertificateDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
    return rootCertificate;
  }
}