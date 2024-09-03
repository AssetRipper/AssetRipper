using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

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
				new AudioTab(Asset, Path),
				new ImageTab(Asset, Path),
				new ModelTab(Asset, Path),
				new TextTab(Asset, Path),
				new FontTab(Asset, Path),
				new YamlTab(Asset, Path),
				new JsonTab(Asset, Path),
				new HexTab(Asset, Path),
				new DependenciesTab(Asset),
				new DevelopmentTab(Asset),
			];

		HtmlTab.WriteNavigation(writer, tabs);
		HtmlTab.WriteContent(writer, tabs);
	}

	protected override void WriteScriptReferences(TextWriter writer)
	{
		base.WriteScriptReferences(writer);
		new Script(writer).WithSrc("https://cdn.babylonjs.com/babylon.js").Close();
		new Script(writer).WithSrc("https://cdn.babylonjs.com/loaders/babylonjs.loaders.min.js").Close();
		new Script(writer).WithSrc("/js/mesh_preview.js").Close();
	}
}
