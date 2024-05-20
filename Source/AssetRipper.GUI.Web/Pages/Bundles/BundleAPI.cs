using AssetRipper.Assets.Bundles;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Bundles;

internal static class BundleAPI
{
	public static class Urls
	{
		public const string Base = "/Bundles";
		public const string View = Base + "/View";
	}

	private const string Path = "Path";

	public static string GetViewUrl(BundlePath path) => $"/Bundles/View?{GetPathQuery(path)}";
	public static Task GetView(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetBundleFromQuery(context, out Bundle? bundle, out BundlePath path, out Task? failureTask))
		{
			return new ViewPage() { Bundle = bundle, Path = path }.WriteToResponse(context.Response);
		}
		else
		{
			return failureTask;
		}
	}

	private static string GetPathQuery(BundlePath path) => $"{Path}={path.ToJson().ToUrl()}";

	private static bool TryGetBundleFromQuery(HttpContext context, [NotNullWhen(true)] out Bundle? bundle, out BundlePath path, [NotNullWhen(false)] out Task? failureTask)
	{
		if (!context.Request.Query.TryGetValue(Path, out string? json) || string.IsNullOrEmpty(json))
		{
			bundle = null;
			path = default;
			failureTask = context.Response.NotFound("The path must be included in the request.");
			return false;
		}

		try
		{
			path = BundlePath.FromJson(json);
		}
		catch (Exception ex)
		{
			bundle = null;
			path = default;
			failureTask = context.Response.NotFound(ex.ToString());
			return false;
		}

		if (!GameFileLoader.IsLoaded)
		{
			bundle = null;
			failureTask = context.Response.NotFound("No files loaded.");
			return false;
		}
		else if (!GameFileLoader.GameBundle.TryGetBundle(path, out bundle))
		{
			failureTask = context.Response.NotFound($"Bundle could not be resolved: {path}");
			return false;
		}
		else
		{
			failureTask = null;
			return true;
		}
	}
}
