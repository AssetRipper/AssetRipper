using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.SourceGenerated.Classes.ClassID_329;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class VideoTab : AssetHtmlTab
{
	public AssetPath Path { get; }
	private string? Extension { get; }
	public override string DisplayName => Localization.AssetTabVideo;
	public override string HtmlName => "video";
	public override bool Enabled => AssetAPI.HasVideoData(Asset);
	public VideoTab(IUnityObjectBase asset, AssetPath path) : base(asset)
	{
		Path = path;
		Extension = (asset as IVideoClip)?.TryGetExtensionFromPath();
	}
	public override void Write(TextWriter writer)
	{
		string videoUrl = AssetAPI.GetVideoUrl(Path);

		using (new Video(writer).WithControls().WithStyle("width:100%; height:100%").End())
		{
			new Source(writer).WithSrc(videoUrl).WithType($"video/{Extension}").Close();
		}

		// Click a button beneath the video to download its data
		using (new Div(writer).WithTextCenter().End())
		{
			SaveButton.Write(writer, videoUrl);
		}
	}
}
