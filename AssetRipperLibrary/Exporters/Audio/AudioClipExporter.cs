using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using System.IO;

namespace AssetRipper.Library.Exporters.Audio
{
	public class AudioClipExporter : BinaryAssetExporter
	{
		private AudioExportFormat AudioFormat { get; set; }
		public AudioClipExporter(LibraryConfiguration configuration) => AudioFormat = configuration.AudioExportFormat;

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Core.Classes.Object.Object asset)
		{
			return new AssetExportCollection(this, asset, GetFileExtension((AudioClip)asset));
		}

		public override bool Export(IExportContainer container, Core.Classes.Object.Object asset, string path)
		{
			AudioClip audioClip = (AudioClip)asset;
			bool success = AudioClipDecoder.TryGetDecodedAudioClipData(audioClip, out byte[] decodedData, out string fileExtension);
			if (!success)
				return false;

			if (fileExtension == "ogg" && AudioFormat == AudioExportFormat.Wav)
				decodedData = AudioClipDecoder.ConvertOggToWav(decodedData);

			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				using (BufferedStream stream = new BufferedStream(fileStream))
				{
					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						writer.Write(decodedData);
					}
				}
			}
			return true;
		}

		public override bool IsHandle(Core.Classes.Object.Object asset)
		{
			return AudioClipDecoder.CanDecode((AudioClip)asset) && AudioFormat != AudioExportFormat.Native;
		}

		private string GetFileExtension(AudioClip audioClip)
		{
			string defaultExtension = AudioClipDecoder.GetFileExtension(audioClip);
			if (AudioFormat == AudioExportFormat.Wav && defaultExtension == "ogg")
				return "wav";
			else
				return defaultExtension;
		}
	}
}
