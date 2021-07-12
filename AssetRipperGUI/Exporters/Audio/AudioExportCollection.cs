using AssetRipper.Project;
using AssetRipper.Classes;
using AssetRipper.Converters;

namespace AssetRipperGUI.Exporters
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
