using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class JsonTab(IUnityObjectBase asset, AssetPath path) : AssetHtmlTab(asset)
{
	public string Url { get; } = AssetAPI.GetJsonUrl(path);
	public string FileName => $"{Asset.GetBestName()}.json";
	public override string DisplayName => Localization.Json;
	public override string HtmlName => "json";
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
