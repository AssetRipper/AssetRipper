using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class TextTab : AssetHtmlTab
{
	public string Url { get; }

	public string? FileName => AssetAPI.GetTextFileName(Asset);

	public override string DisplayName => Localization.AssetTabText;

	public override string HtmlName => "text";

	public override bool Enabled => AssetAPI.HasText(Asset);

	public TextTab(IUnityObjectBase asset, AssetPath path) : base(asset)
	{
		Url = AssetAPI.GetTextUrl(path);
	}

	public override void Write(TextWriter writer)
	{
		new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").WithDynamicTextContent(Url).Close();
		using (new Div(writer).WithClass("text-center").End())
		{
			SaveButton.Write(writer, Url, FileName);
		}
	}
}
