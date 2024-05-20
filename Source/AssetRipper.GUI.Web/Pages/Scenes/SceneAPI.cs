using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Scenes;

internal static class SceneAPI
{
	public static class Urls
	{
		public const string Base = "/Scenes";
		public const string View = Base + "/View";
	}

	private const string Path = "Path";

	public static string GetViewUrl(ScenePath path) => $"/Scenes/View?{GetPathQuery(path)}";
	public static Task GetView(HttpContext context)
	{
		context.Response.DisableCaching();
		if (TryGetSceneFromQuery(context, out SceneDefinition? scene, out ScenePath path, out Task? failureTask))
		{
			return new ViewPage() { Scene = scene, Path = path }.WriteToResponse(context.Response);
		}
		else
		{
			return failureTask;
		}
	}

	private static string GetPathQuery(ScenePath path) => $"{Path}={path.ToJson().ToUrl()}";

	private static bool TryGetSceneFromQuery(HttpContext context, [NotNullWhen(true)] out SceneDefinition? scene, out ScenePath path, [NotNullWhen(false)] out Task? failureTask)
	{
		if (!context.Request.Query.TryGetValue(Path, out string? json) || string.IsNullOrEmpty(json))
		{
			scene = null;
			path = default;
			failureTask = context.Response.NotFound("The path must be included in the request.");
			return false;
		}

		try
		{
			path = ScenePath.FromJson(json);
		}
		catch (Exception ex)
		{
			scene = null;
			path = default;
			failureTask = context.Response.NotFound(ex.ToString());
			return false;
		}

		if (!GameFileLoader.IsLoaded)
		{
			scene = null;
			failureTask = context.Response.NotFound("No files loaded.");
			return false;
		}
		else if (!GameFileLoader.GameBundle.TryGetCollection(path.FirstCollection, out AssetCollection? collection))
		{
			scene = null;
			failureTask = context.Response.NotFound($"Scene could not be resolved: {path.FirstCollection}");
			return false;
		}
		else if (!collection.IsScene)
		{
			scene = null;
			failureTask = context.Response.NotFound($"Collection is not a scene: {path.FirstCollection}");
			return false;
		}
		else
		{
			scene = collection.Scene;
			failureTask = null;
			return true;
		}
	}
}
