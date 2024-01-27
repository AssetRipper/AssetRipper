using AssetRipper.Assets;
using AssetRipper.Export.PrimaryContent;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class JsonTab(IUnityObjectBase asset) : HtmlTab
{
	public string Text { get; } = GetJsonString(asset);
	public string FileName { get; } = $"{asset.GetBestName()}.json";
	public override string DisplayName => Localization.Json;
	public override string HtmlName => "json";
	public override bool Enabled => Text.Length > 4;

	public override void Write(TextWriter writer)
	{
		new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(Text);
		using (new Div(writer).WithClass("text-center").End())
		{
			TextSaveButton.Write(writer, FileName, Text);
		}
	}

	private static string GetJsonString(IUnityObjectBase asset)
	{
		return new DefaultJsonWalker().SerializeStandard(asset);
	}
}
