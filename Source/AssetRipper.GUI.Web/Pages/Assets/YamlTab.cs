using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class YamlTab(IUnityObjectBase asset) : HtmlTab
{
	public string Text { get; } = GetYamlString(asset);
	public string FileName { get; } = $"{asset.GetBestName()}.asset";
	public override string DisplayName => Localization.Yaml;
	public override string HtmlName => "yaml";
	public override bool Enabled => !string.IsNullOrEmpty(Text);

	public override void Write(TextWriter writer)
	{
		new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(Text);
		using (new Div(writer).WithClass("text-center").End())
		{
			TextSaveButton.Write(writer, FileName, Text);
		}
	}

	private static string GetYamlString(IUnityObjectBase asset)
	{
		return new StringYamlWalker().AppendEditor(asset, ExportIdHandler.GetMainExportID(asset)).ToString()!;
	}
}
