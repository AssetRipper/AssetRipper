using AssetRipper.GUI.Web.Paths;
using AssetRipper.IO.Files;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.FailedFiles;

internal static class FailedFileAPI
{
	public static class Urls
	{
		public const string Base = "/FailedFiles";
		public const string View = Base + "/View";
		public const string StackTrace = Base + "/StackTrace";
	}

	private const string Path = "Path";

	public static string GetViewUrl(FailedFilePath path) => $"{Urls.View}?{GetPathQuery(path)}";
	public static Task GetView(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetFileFromQuery(context, out FailedFile? file, out FailedFilePath path, out Task? failureTask))
		{
			return new ViewPage() { File = file, Path = path }.WriteToResponse(context.Response);
		}
		else
		{
			return failureTask;
		}
	}

	public static string GetStackTraceUrl(FailedFilePath path) => $"{Urls.StackTrace}?{GetPathQuery(path)}";
	public static Task GetStackTrace(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetFileFromQuery(context, out FailedFile? file, out _, out Task? failureTask))
		{
			return Results.Text(file.StackTrace).ExecuteAsync(context);
		}
		else
		{
			return failureTask;
		}
	}

	private static string GetPathQuery(FailedFilePath path) => $"{Path}={path.ToJson().ToUrl()}";

	private static bool TryGetFileFromQuery(HttpContext context, [NotNullWhen(true)] out FailedFile? file, out FailedFilePath path, [NotNullWhen(false)] out Task? failureTask)
	{
		if (!context.Request.Query.TryGetValue(Path, out string? json) || string.IsNullOrEmpty(json))
		{
			file = null;
			path = default;
			failureTask = context.Response.NotFound("The path must be included in the request.");
			return false;
		}

		try
		{
			path = FailedFilePath.FromJson(json);
		}
		catch (Exception ex)
		{
			file = null;
			path = default;
			failureTask = context.Response.NotFound(ex.ToString());
			return false;
		}

		if (!GameFileLoader.IsLoaded)
		{
			file = null;
			failureTask = context.Response.NotFound("No files loaded.");
			return false;
		}
		else if (!GameFileLoader.GameBundle.TryGetFailedFile(path, out file))
		{
			failureTask = context.Response.NotFound($"Resource file could not be resolved: {path}");
			return false;
		}
		else
		{
			failureTask = null;
			return true;
		}
	}
}
