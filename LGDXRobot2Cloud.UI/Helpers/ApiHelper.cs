using LGDXRobot2Cloud.UI.Client.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace LGDXRobot2Cloud.UI.Helpers;

public static class ApiHelper
{
	public static readonly string UnexpectedResponseStatusCodeMessage = "The service returned an unexpected error. Status Code: ";
	public static readonly string ApiErrorMessage = "An error occurred while calling the API";

	public static Dictionary<string,string> GenerateErrorDictionary(ApiException apiException)
	{
		Dictionary<string, string> errorDictionary = [];
		if (apiException is ValidationProblemDetails validationProblemDetails)
		{
			if (validationProblemDetails.Errors != null)
			{
				foreach (var error in validationProblemDetails.Errors.AdditionalData)
				{
					if (error.Value is UntypedArray arrayValue)
					{
						var message = arrayValue.GetValue().First();
						if (message is UntypedString stringValue)
						{
							errorDictionary.Add(error.Key, stringValue.GetValue() ?? string.Empty);
						}
					}
				}
			}
		}
		else if (apiException is ProblemDetails problemDetails)
		{
			errorDictionary.Add("", $"The service returned an unexpected error: {problemDetails.Title}");
		}
		return errorDictionary;
	}
}