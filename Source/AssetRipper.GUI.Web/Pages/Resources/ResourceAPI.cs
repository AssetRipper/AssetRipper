using AssetRipper.GUI.Web.Paths;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Resources;

internal static class ResourceAPI
{
	public static class Urls
	{
		public const string Base = "/Resources";
		public const string View = Base + "/View";
		public const string Data = Base + "/Data";
	}

	private const string Path = "Path";

	public static string GetViewUrl(ResourcePath path) => $"{Urls.View}?{GetPathQuery(path)}";
	public static Task GetView(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetResourceFromQuery(context, out ResourceFile? resource, out ResourcePath path, out Task? failureTask))
		{
			return new ViewPage() { Resource = resource, Path = path }.WriteToResponse(context.Response);
		}
		else
		{
			return failureTask;
		}
	}

	public static string GetDataUrl(ResourcePath path) => $"{Urls.Data}?{GetPathQuery(path)}";
	public static Task GetData(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetResourceFromQuery(context, out ResourceFile? resource, out Task? failureTask))
		{
			return Results.Bytes(resource.ToByteArray(), "application/octet-stream").ExecuteAsync(context);
		}
		else
		{
			return failureTask;
		}
	}

	private static string GetPathQuery(ResourcePath path) => $"{Path}={path.ToJson().ToUrl()}";

	private static bool TryGetResourceFromQuery(HttpContext context, [NotNullWhen(true)] out ResourceFile? resource, [NotNullWhen(false)] out Task? failureTask)
	{
		return TryGetResourceFromQuery(context, out resource, out _, out failureTask);
	}

	private static bool TryGetResourceFromQuery(HttpContext context, [NotNullWhen(true)] out ResourceFile? resource, out ResourcePath path, [NotNullWhen(false)] out Task? failureTask)
	{
		if (!context.Request.Query.TryGetValue(Path, out string? json) || string.IsNullOrEmpty(json))
		{
			resource = null;
			path = default;
			failureTask = context.Response.NotFound("The path must be included in the request.");
			return false;
		}

		try
		{
			path = ResourcePath.FromJson(json);
		}
		catch (Exception ex)
		{
			resource = null;
			path = default;
			failureTask = context.Response.NotFound(ex.ToString());
			return false;
		}

		if (!GameFileLoader.IsLoaded)
		{
			resource = null;
			failureTask = context.Response.NotFound("No files loaded.");
			return false;
		}
		else if (!GameFileLoader.GameBundle.TryGetResource(path, out resource))
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
