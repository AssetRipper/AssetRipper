using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class YamlTab(IUnityObjectBase asset, AssetPath path) : AssetHtmlTab(asset)
{
	public string Url { get; } = AssetAPI.GetYamlUrl(path);
	public string FileName { get; } = $"{asset.GetBestName()}.asset";
	public override string DisplayName => Localization.Yaml;
	public override string HtmlName => "yaml";
	public override bool Enabled => true;

	public override void Write(TextWriter writer)
	{
		new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").WithDynamicTextContent(Url).Close();
		using (new Div(writer).WithClass("text-center").End())
		{
			SaveButton.Write(writer, Url, FileName);
		}
	}
}
