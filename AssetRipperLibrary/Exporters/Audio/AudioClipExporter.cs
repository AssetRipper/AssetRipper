using AssetRipper.Core;
using AssetRipper.Core.Classes.AudioClip;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using System.IO;

namespace AssetRipper.Library.Exporters.Audio
{
	public class AudioClipExporter : BinaryAssetExporter
	{
		private AudioExportFormat AudioFormat { get; set; }
		public AudioClipExporter(LibraryConfiguration configuration) => AudioFormat = configuration.AudioExportFormat;

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, GetFileExtension((IAudioClip)asset));
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			IAudioClip audioClip = (IAudioClip)asset;
			bool success = AudioClipDecoder.TryGetDecodedAudioClipData(audioClip, out byte[] decodedData, out string fileExtension);
			if (!success)
				return false;

			if (AudioFormat == AudioExportFormat.Wav || AudioFormat == AudioExportFormat.Mp3)
			{
				if (fileExtension == "ogg")
					decodedData = AudioConverter.OggToWav(decodedData);

				if (AudioFormat == AudioExportFormat.Mp3 && System.OperatingSystem.IsWindows() && (fileExtension == "ogg" || fileExtension == "wav"))
					decodedData = AudioConverter.WavToMp3(decodedData);
			}

			if (decodedData == null || decodedData.Length == 0)
				return false;

			TaskManager.AddTask(File.WriteAllBytesAsync(path, decodedData));
			return true;
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IAudioClip audio && AudioClipDecoder.CanDecode(audio) && AudioFormat != AudioExportFormat.Native;
		}

		private string GetFileExtension(IAudioClip audioClip)
		{
			string defaultExtension = AudioClipDecoder.GetFileExtension(audioClip);
			if (AudioFormat == AudioExportFormat.Wav && defaultExtension == "ogg")
				return "wav";
			if (AudioFormat == AudioExportFormat.Mp3 && System.OperatingSystem.IsWindows() && (defaultExtension == "ogg" || defaultExtension == "wav"))
				return "mp3";
			else
				return defaultExtension;
		}
	}
}
