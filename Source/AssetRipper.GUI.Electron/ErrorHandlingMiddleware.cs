using AssetRipper.Import.Logging;

namespace AssetRipper.GUI.Electron;

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

			context.Response.Redirect("/Error"); // Redirect to the custom error page.
		}
	}
}
