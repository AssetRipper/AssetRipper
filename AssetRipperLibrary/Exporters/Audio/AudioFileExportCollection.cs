using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Collections;
using AssetRipper.Library.Configuration;

namespace AssetRipper.Library.Exporters.Audio
{
	internal class AudioFileExportCollection : AssetExportCollection
	{
		private AudioExportFormat AudioFormat { get; set; }

		public AudioFileExportCollection(IAssetExporter assetExporter, AudioClip asset, AudioExportFormat audioExportFormat) : base(assetExporter, asset)
		{
			AudioFormat = audioExportFormat;
		}

		protected override string GetExportExtension(Core.Classes.Object.Object asset)
		{
			if (AudioFormat == AudioExportFormat.Wav)
				return "wav";
			else
				return "ogg";
		}
	}
}