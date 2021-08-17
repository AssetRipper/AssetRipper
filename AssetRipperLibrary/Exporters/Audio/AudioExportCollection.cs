using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Structure.Collections;
using System.Runtime.Versioning;

namespace AssetRipper.Library.Exporters.Audio
{
	[SupportedOSPlatform("windows")]
	public class AudioExportCollection : AssetExportCollection
	{
		public AudioExportCollection(IAssetExporter assetExporter, AudioClip asset) : base(assetExporter, asset) { }

		protected override string GetExportExtension(Core.Classes.Object.Object asset)
		{
			AudioClip audioClip = (AudioClip)asset;
			return AudioAssetExporter.IsSupported(audioClip) ? "wav" : audioClip.ExportExtension;
		}
	}
}
