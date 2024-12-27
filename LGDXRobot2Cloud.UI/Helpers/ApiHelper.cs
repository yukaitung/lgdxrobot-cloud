namespace LGDXRobot2Cloud.UI.Helpers;

public record ApiResponse<T>
{
	public T? Data { get; set; }
	public IDictionary<string,string[]>? Errors { get; set; }
	public required bool IsSuccess { get; set; }
}

public static class ApiHelper
{
	public static readonly string UnexpectedResponseStatusCodeMessage = "The service returned an unexpected error. Status Code: ";
	public static readonly string ApiErrorMessage = "An error occurred while calling the API";
}