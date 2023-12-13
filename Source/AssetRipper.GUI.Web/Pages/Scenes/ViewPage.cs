using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Scenes;

public sealed class ViewPage : DefaultPage
{
	public required SceneDefinition Scene { get; init; }
	public required ScenePath Path { get; init; }

	public override string GetTitle() => Scene.Name;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());
		using (new Table(writer).WithClass("table").End())
		{
			using (new Tbody(writer).End())
			{
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.Name);
					new Td(writer).Close(Scene.Name);
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.Path);
					new Td(writer).Close(Scene.Path);
				}
				using (new Tr(writer).End())
				{
					new Th(writer).Close(Localization.Guid);
					new Td(writer).Close(Scene.GUID.ToString());
				}
			}
		}
		new H2(writer).Close(Localization.Collections);
		using (new Ul(writer).End())
		{
			foreach (AssetCollection collection in Scene.Collections)
			{
				using (new Li(writer).End())
				{
					PathLinking.WriteLink(writer, collection);
				}
			}
		}
	}

	public static Task HandlePostRequest(HttpContext context)
	{
		string? json = context.Request.Form[PathLinking.FormKey];
		if (string.IsNullOrEmpty(json))
		{
			return context.Response.NotFound();
		}

		ScenePath path;
		try
		{
			path = ScenePath.FromJson(json);
		}
		catch (Exception ex)
		{
			return context.Response.NotFound(ex.ToString());
		}

		if (!GameFileLoader.IsLoaded)
		{
			return context.Response.NotFound("No files loaded.");
		}
		else if (!GameFileLoader.GameBundle.TryGetCollection(path.FirstCollection, out AssetCollection? collection))
		{
			return context.Response.NotFound($"Scene could not be resolved: {path.FirstCollection}");
		}
		else if (!collection.IsScene)
		{
			return context.Response.NotFound($"Collection is not a scene: {path.FirstCollection}");
		}
		else
		{
			return new ViewPage() { Scene = collection.Scene, Path = path }.WriteToResponse(context.Response);
		}
	}
}
