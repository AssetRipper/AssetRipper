using AssetRipper.Assets.Bundles;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Resources;

public sealed class ViewPage : DefaultPage
{
	public required ResourceFile Resource { get; init; }
	public required ResourcePath Path { get; init; }

	public Bundle? Bundle => GameFileLoader.GameBundle.TryGetBundle(Path.BundlePath);

	public override string GetTitle() => Resource.NameFixed;

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		if (Bundle is { } bundle)
		{
			new H2(writer).Close(Localization.Bundle);
			PathLinking.WriteLink(writer, Path.BundlePath, bundle.Name);
		}

		//This upper limit should probably be smaller.
		if (Resource.Stream.Length < int.MaxValue)
		{
			using (new Div(writer).WithClass("text-center").End())
			{
				DataSaveButton.Write(writer, Resource.NameFixed, Resource.ToByteArray());
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

		ResourcePath path;
		try
		{
			path = ResourcePath.FromJson(json);
		}
		catch (Exception ex)
		{
			return context.Response.NotFound(ex.ToString());
		}

		if (!GameFileLoader.IsLoaded)
		{
			return context.Response.NotFound("No files loaded.");
		}
		else if (!GameFileLoader.GameBundle.TryGetResource(path, out ResourceFile? resource))
		{
			return context.Response.NotFound($"Resource file could not be resolved: {path}");
		}
		else
		{
			return new ViewPage() { Resource = resource, Path = path }.WriteToResponse(context.Response);
		}
	}
}
