using AssetRipper.Import.Logging;

namespace AssetRipper.GUI.Electron;

internal sealed class ErrorHandlingMiddleware
{
	private readonly RequestDelegate next;

	public ErrorHandlingMiddleware(RequestDelegate next)
	{
		this.next = next;
	}

	public async Task Invoke(HttpContext context)
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
