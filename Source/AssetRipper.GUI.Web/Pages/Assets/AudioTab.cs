using AssetRipper.Assets;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class AudioTab : AssetHtmlTab
{
	public string Source { get; }

	public override string DisplayName => Localization.AssetTabAudio;

	public override string HtmlName => "audio";

	public override bool Enabled => AssetAPI.HasAudioData(Asset);

	public AudioTab(IUnityObjectBase asset, AssetPath path) : base(asset)
	{
		Source = AssetAPI.GetAudioUrl(path);
	}

	public override void Write(TextWriter writer)
	{
		using (new Table(writer).WithCustomAttribute("width", "100%").WithCustomAttribute("height", "100%").End())
		{
			using (new Tbody(writer).End())
			{
				using (new Tr(writer).End())
				{
					using (new Td(writer).WithAlign("center").WithCustomAttribute("valign", "middle").End())
					{
						new Audio(writer).WithControls("").WithPreload("auto").WithClass("mt-4").WithSrc(Source).Close();
					}
				}
			}
		}
	}
}
