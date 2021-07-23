using AssetRipper.Converters.Project.Exporters;
using AssetRipper.Parser.Classes.AudioClip;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Structure.Collections;
using System.Runtime.Versioning;

namespace AssetRipperLibrary.Exporters.Audio
{
	[SupportedOSPlatform("windows")]
	public class AudioExportCollection : AssetExportCollection
	{
		public AudioExportCollection(IAssetExporter assetExporter, AudioClip asset) :
			base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(Object asset)
		{
			AudioClip audioClip = (AudioClip)asset;
			return AudioAssetExporter.IsSupported(audioClip) ? "wav" : audioClip.ExportExtension;
		}
	}
}
