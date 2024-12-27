using Microsoft.AspNetCore.Mvc;

namespace LGDXRobot2Cloud.UI.Helpers;

public record ApiResponse<T>
{
	public T? Data { get; set; }
	public ValidationProblemDetails? ValidationErrors { get; set; }
	public required bool IsSuccess { get; set; }
}

public static class ApiHelper
{
	public static readonly string UnexpectedResponseStatusCodeMessage = "Unexpected response status code: ";
	public static readonly string ApiErrorMessage = "An error occurred while calling the API";
}