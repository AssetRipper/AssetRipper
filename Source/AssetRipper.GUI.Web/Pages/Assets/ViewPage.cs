using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Web.Extensions;
using Microsoft.AspNetCore.Http;

namespace AssetRipper.GUI.Web.Pages.Assets;

public sealed class ViewPage : DefaultPage
{
	public required IUnityObjectBase Asset { get; init; }
	public required AssetPath Path { get; init; }

	public override string GetTitle() => Asset.GetBestName();

	public override void WriteInnerContent(TextWriter writer)
	{
		new H1(writer).Close(GetTitle());

		ReadOnlySpan<HtmlTab> tabs =
			[
				new InformationTab(Asset, Path),
				new AudioTab(Asset),
				new ImageTab(Asset),
				new TextTab(Asset),
				new FontTab(Asset),
				new YamlTab(Asset),
				new JsonTab(Asset),
				new HexTab(Asset),
				new DependenciesTab(Asset),
				new DevelopmentTab(Asset),
			];

		HtmlTab.WriteNavigation(writer, tabs);
		HtmlTab.WriteContent(writer, tabs);
	}

	public static Task HandlePostRequest(HttpContext context)
	{
		string? json = context.Request.Form[PathLinking.FormKey];
		if (string.IsNullOrEmpty(json))
		{
			return context.Response.NotFound("The path must be included in the request.");
		}

		AssetPath path;
		try
		{
			path = AssetPath.FromJson(json);
		}
		catch (Exception ex)
		{
			return context.Response.NotFound(ex.ToString());
		}

		if (!GameFileLoader.IsLoaded)
		{
			return context.Response.NotFound("No files loaded.");
		}
		else if (!GameFileLoader.GameBundle.TryGetAsset(path, out IUnityObjectBase? asset))
		{
			return context.Response.NotFound($"Asset could not be resolved: {path}");
		}
		else
		{
			return new ViewPage() { Asset = asset, Path = path }.WriteToResponse(context.Response);
		}
	}
}
