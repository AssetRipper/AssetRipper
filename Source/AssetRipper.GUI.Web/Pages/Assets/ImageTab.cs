using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class ImageTab : HtmlTab
{
	public override string DisplayName => Localization.AssetTabImage;

	public override string HtmlName => "image";

	public override bool Enabled => AssetAPI.HasImageData(Asset);

	public IUnityObjectBase Asset { get; }

	public ImageTab(IUnityObjectBase asset)
	{
		Asset = asset;
	}

	public override void Write(TextWriter writer)
	{
		AssetPath assetPath = Asset.GetPath();
		string pngUrl = AssetAPI.GetImageUrl(assetPath, "png");
		string rawUrl = AssetAPI.GetImageUrl(assetPath);
		string fileName = FileUtils.FixInvalidNameCharacters(Asset.GetBestName());

		// Click on image to save
		using (new A(writer).WithHref(pngUrl).WithDownload(fileName).End())
		{
			new Img(writer).WithSrc(pngUrl).WithStyle("object-fit:contain; width:100%; height:100%").Close();
		}

		// Click a button beneath the image to download its raw data
		using (new Div(writer).WithTextCenter().End())
		{
			DataSaveButton.Write(writer, rawUrl, fileName, Localization.SaveRawData);
		}
	}
}
