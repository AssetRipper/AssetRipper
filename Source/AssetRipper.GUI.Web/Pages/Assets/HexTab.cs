using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class HexTab : AssetHtmlTab
{
	public string Url { get; }

	public string FileName => $"{Asset.GetBestName()}.dat";

	public override string DisplayName => Localization.AssetTabHex;

	public override string HtmlName => "hex";

	public override bool Enabled => AssetAPI.HasBinaryData(Asset);

	public HexTab(IUnityObjectBase asset, AssetPath path) : base(asset)
	{
		Url = Enabled ? AssetAPI.GetBinaryUrl(path) : "";
	}

	public override void Write(TextWriter writer)
	{
		using (new Div(writer).WithClass("text-center").End())
		{
			SaveButton.Write(writer, Url, FileName);
		}
	}
}
