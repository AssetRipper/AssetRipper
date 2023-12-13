using AssetRipper.Import.Logging;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web;

internal sealed class ErrorHandlingMiddleware : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			Logger.Error(ex);

			context.Response.Redirect("/"); // Redirect to the main page since we don't have an error page.
		}
	}
}
