using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.IO.Files;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class ImageTab : AssetHtmlTab
{
	public AssetPath Path { get; }

	public override string DisplayName => Localization.AssetTabImage;

	public override string HtmlName => "image";

	public override bool Enabled => AssetAPI.HasImageData(Asset);

	public ImageTab(IUnityObjectBase asset, AssetPath path) : base(asset)
	{
		Path = path;
	}

	public override void Write(TextWriter writer)
	{
		string pngUrl = AssetAPI.GetImageUrl(Path, "png");
		string rawUrl = AssetAPI.GetImageUrl(Path);
		string fileName = FileSystem.FixInvalidFileNameCharacters(Asset.GetBestName());

		// Click on image to save
		using (new A(writer).WithHref(pngUrl).WithDownload(fileName).End())
		{
			new Img(writer).WithSrc(pngUrl).WithStyle("object-fit:contain; width:100%; height:100%").Close();
		}

		// Click a button beneath the image to download its raw data
		using (new Div(writer).WithTextCenter().End())
		{
			SaveButton.Write(writer, rawUrl, fileName, Localization.SaveRawData);
		}
	}
}
