using Microsoft.AspNetCore.Http;

namespace AssetRipper.Web.Extensions;

public static class HttpResponseExtensions
{
	public static void DisableCaching(this HttpResponse response)
	{
		response.Headers.CacheControl = "no-store, max-age=0";
	}

	public static Task NotFound(this HttpResponse response, string? errorMessage = null)
	{
		// Set the response status code to 404 (Not Found)
		response.StatusCode = 404;

		// Optionally, we can provide a custom error message
		if (!string.IsNullOrEmpty(errorMessage))
		{
			return response.WriteAsync(errorMessage);
		}
		else
		{
			return Task.CompletedTask;
		}
	}
}
