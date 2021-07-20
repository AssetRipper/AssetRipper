using AssetRipper.Converters;
using AssetRipper.Converters.Project.Exporter;
using AssetRipper.Parser.Classes.AudioClip;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Structure.ProjectCollection.Collections;

namespace AssetRipperLibrary.Exporters.Audio
{
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
