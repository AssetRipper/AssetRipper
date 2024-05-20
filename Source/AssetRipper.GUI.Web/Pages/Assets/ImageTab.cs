using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

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
		string sourcePath = $"/Assets/Image?{AssetAPI.Path}={Asset.GetPath().ToJson().ToUrl()}&{AssetAPI.Extension}=png";

		// Click on image to save
		using (new A(writer).WithHref(sourcePath).WithDownload("extracted_image").End())
		{
			new Img(writer).WithSrc(sourcePath).WithStyle("object-fit:contain; width:100%; height:100%").Close();
		}

		// Todo: add a button beneath the image to download its raw data
		// https://github.com/AssetRipper/AssetRipper/issues/1298
	}
}
