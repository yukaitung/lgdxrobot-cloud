using LGDXRobot2Cloud.Data.Models.DTOs.V1.Requests;
using LGDXRobot2Cloud.Data.Models.DTOs.V1.Responses;
using LGDXRobot2Cloud.UI.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LGDXRobot2Cloud.UI.Services;

public interface IAuthService
{
  Task<ApiResponse<bool>> LoginAsync(HttpContext context, LoginRequestDto loginRequestDto);
  Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
  Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
}

public sealed class AuthService : IAuthService
{
  private readonly HttpClient _httpClient;
  private readonly JsonSerializerOptions _jsonSerializerOptions;

  public AuthService(HttpClient httpClient)
  {
    _httpClient = httpClient;
    _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
  }

  public async Task<ApiResponse<bool>> LoginAsync(HttpContext context, LoginRequestDto loginRequestDto)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(loginRequestDto), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("/Identity/Auth/Login", content);
      if (response.IsSuccessStatusCode)
      {
        var loginResponseDto = await JsonSerializer.DeserializeAsync<LoginResponseDto>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponseDto!.AccessToken);
        var refreshToken = new JwtSecurityTokenHandler().ReadJwtToken(loginResponseDto!.RefreshToken);
        var identity = new ClaimsIdentity([], CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaims(accessToken.Claims);
        identity.AddClaim(new Claim("access_token", loginResponseDto.AccessToken));
        identity.AddClaim(new Claim("refresh_token", loginResponseDto.RefreshToken));
        var user = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties{
          IsPersistent = false,
          ExpiresUtc = refreshToken.ValidTo
        };
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user, authProperties);
        return new ApiResponse<bool> {
          Data = response.IsSuccessStatusCode,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<bool> {
          Errors = validationProblemDetails?.Errors,
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

  public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("/Identity/Auth/ForgotPassword", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = response.IsSuccessStatusCode,
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

  public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request)
  {
    try
    {
      var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
      var response = await _httpClient.PostAsync("/Identity/Auth/ResetPassword", content);
      if (response.IsSuccessStatusCode)
      {
        return new ApiResponse<bool> {
          Data = response.IsSuccessStatusCode,
          IsSuccess = response.IsSuccessStatusCode
        };
      }
      else if (response.StatusCode == HttpStatusCode.BadRequest)
      {
        var validationProblemDetails = await JsonSerializer.DeserializeAsync<ValidationProblemDetails>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
        return new ApiResponse<bool> {
          Errors = validationProblemDetails?.Errors,
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