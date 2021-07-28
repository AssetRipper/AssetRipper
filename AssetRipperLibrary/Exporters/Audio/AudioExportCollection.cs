using AssetRipper.Project.Exporters;
using AssetRipper.Classes.AudioClip;
using AssetRipper.Classes.Object;
using AssetRipper.Structure.Collections;
using System.Runtime.Versioning;

namespace AssetRipperLibrary.Exporters.Audio
{
	[SupportedOSPlatform("windows")]
	public class AudioExportCollection : AssetExportCollection
	{
		public AudioExportCollection(IAssetExporter assetExporter, AudioClip asset) : base(assetExporter, asset) { }

		protected override string GetExportExtension(UnityObject asset)
		{
			AudioClip audioClip = (AudioClip)asset;
			return AudioAssetExporter.IsSupported(audioClip) ? "wav" : audioClip.ExportExtension;
		}
	}
}
