using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Audio;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class AudioTab : HtmlTab
{
	public string Source { get; }

	public override string DisplayName => Localization.AssetTabAudio;

	public override string HtmlName => "audio";

	public override bool Enabled => !string.IsNullOrEmpty(Source);

	public AudioTab(IUnityObjectBase asset)
	{
		Source = TryDecode(asset);
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
						new Audio(writer).WithControls("").WithClass("mt-4").WithSrc(Source).Close();
					}
				}
			}
		}
	}

	private static string TryDecode(IUnityObjectBase asset)
	{
		if (asset is IAudioClip clip && AudioClipDecoder.TryDecode(clip, out byte[]? decodedAudioData, out string? extension, out _))
		{
			return $"data:audio/{extension};base64,{Convert.ToBase64String(decodedAudioData, Base64FormattingOptions.None)}";
		}
		return "";
	}
}
