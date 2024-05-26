using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class FontTab : AssetHtmlTab
{
	public string Url { get; }

	public override string DisplayName => Localization.AssetTabFont;

	public override string HtmlName => "font";

	public override bool Enabled => AssetAPI.HasFontData(Asset);

	public FontTab(IUnityObjectBase asset, AssetPath path) : base(asset)
	{
		Url = AssetAPI.GetFontUrl(path);
	}

	public override void Write(TextWriter writer)
	{
		using (new Div(writer).WithClass("text-center").End())
		{
			new H1(writer).WithStyle($"font-family: {Asset.GetBestName()}").Close("Preview Font (0, 1, 2, 3, 4, 5, 6, 7, 8, 9)");

			SaveButton.Write(writer, Url);

			new Script(writer).Close(
				$$"""
				  const fontFace = new FontFace(`{{Asset.GetBestName()}}`, `url({{Url}})`);
				  document.fonts.add(fontFace);
				  fontFace.load().then().catch(function(error) {
				    console.error(`Font loading failed: ${error}`);
				  });
				  """);
		}
	}
}
