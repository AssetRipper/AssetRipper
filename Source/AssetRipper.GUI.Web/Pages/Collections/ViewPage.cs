using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Collections;

public sealed class ViewPage : DefaultPage
{
	public required AssetCollection Collection { get; init; }
	public required CollectionPath Path { get; init; }

	public override string GetTitle() => Collection.Name;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		new H2(writer).Close(Localization.Bundle);
		PathLinking.WriteLink(writer, Path.BundlePath, Collection.Bundle.Name);

		if (Collection.IsScene)
		{
			new H2(writer).Close(Localization.Scene);
			PathLinking.WriteLink(writer, (ScenePath)Path, Collection.Scene.Name);
		}

		if (Collection.Count > 0)
		{
			new H2(writer).Close(Localization.Assets);
			using (new Table(writer).WithClass("table").End())
			{
				using (new Thead(writer).End())
				{
					using (new Tr(writer).End())
					{
						new Th(writer).Close(Localization.PathId);
						new Th(writer).Close(Localization.Class);
						new Th(writer).Close(Localization.Name);
					}
				}
				using (new Tbody(writer).End())
				{
					foreach (IUnityObjectBase asset in Collection)
					{
						using (new Tr(writer).End())
						{
							new Td(writer).Close(asset.PathID.ToString());
							new Td(writer).Close(asset.ClassName);
							using (new Td(writer).End())
							{
								PathLinking.WriteLink(writer, Path.GetAsset(asset.PathID), asset.GetBestName());
							}
						}
					}
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

		CollectionPath path;
		try
		{
			path = CollectionPath.FromJson(json);
		}
		catch (Exception ex)
		{
			return context.Response.NotFound(ex.ToString());
		}

		if (!GameFileLoader.IsLoaded)
		{
			return context.Response.NotFound("No files loaded.");
		}
		else if (!GameFileLoader.GameBundle.TryGetCollection(path, out AssetCollection? collection))
		{
			return context.Response.NotFound($"Asset collection could not be resolved: {path}");
		}
		else
		{
			return new ViewPage() { Collection = collection, Path = path }.WriteToResponse(context.Response);
		}
	}
}
