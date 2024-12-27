using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.UI.Helpers;

public record ApiResponse<T>
{
	public T? Data { get; set; }
	public IDictionary<string,string[]>? Errors { get; set; }
	public required bool IsSuccess { get; set; }
}

public static class ApiHelper
{
	public static ApiResponse<T> ReturnUnexpectedResponseStatusCode<T>()
	{
		return new ApiResponse<T> {
			IsSuccess = false,
			Errors = new Dictionary<string, string[]> {
				{ "Api", ["The service returned an unexpected error."] }
			}
		};
	}

	public static readonly string ApiErrorMessage = "An error occurred while calling the API";
}