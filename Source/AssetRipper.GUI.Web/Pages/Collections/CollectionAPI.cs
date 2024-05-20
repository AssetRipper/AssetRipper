using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Collections;

internal static class CollectionAPI
{
	public static class Urls
	{
		public const string Base = "/Collections";
		public const string View = Base + "/View";
		public const string Count = Base + "/Count";
	}

	private const string Path = "Path";

	public static string GetViewUrl(CollectionPath path) => $"{Urls.View}?{GetPathQuery(path)}";
	public static Task GetView(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetCollectionFromQuery(context, out AssetCollection? collection, out CollectionPath path, out Task? failureTask))
		{
			return new ViewPage() { Collection = collection, Path = path }.WriteToResponse(context.Response);
		}
		else
		{
			return failureTask;
		}
	}

	public static string GetCountUrl(CollectionPath path) => $"/Collections/Count?{GetPathQuery(path)}";
	public static Task GetCount(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetCollectionFromQuery(context, out AssetCollection? collection, out Task? failureTask))
		{
			return Results.Json(collection.Count, AppJsonSerializerContext.Default.Int32).ExecuteAsync(context);
		}
		else
		{
			return failureTask;
		}
	}

	private static string GetPathQuery(CollectionPath path) => $"{Path}={path.ToJson().ToUrl()}";

	private static bool TryGetCollectionFromQuery(HttpContext context, [NotNullWhen(true)] out AssetCollection? collection, [NotNullWhen(false)] out Task? failureTask)
	{
		return TryGetCollectionFromQuery(context, out collection, out _, out failureTask);
	}

	private static bool TryGetCollectionFromQuery(HttpContext context, [NotNullWhen(true)] out AssetCollection? collection, out CollectionPath path, [NotNullWhen(false)] out Task? failureTask)
	{
		if (!context.Request.Query.TryGetValue(Path, out string? json) || string.IsNullOrEmpty(json))
		{
			collection = null;
			path = default;
			failureTask = context.Response.NotFound("The path must be included in the request.");
			return false;
		}

		try
		{
			path = CollectionPath.FromJson(json);
		}
		catch (Exception ex)
		{
			collection = null;
			path = default;
			failureTask = context.Response.NotFound(ex.ToString());
			return false;
		}

		if (!GameFileLoader.IsLoaded)
		{
			collection = null;
			failureTask = context.Response.NotFound("No files loaded.");
			return false;
		}
		else if (!GameFileLoader.GameBundle.TryGetCollection(path, out collection))
		{
			failureTask = context.Response.NotFound($"Asset collection could not be resolved: {path}");
			return false;
		}
		else
		{
			failureTask = null;
			return true;
		}
	}
}
