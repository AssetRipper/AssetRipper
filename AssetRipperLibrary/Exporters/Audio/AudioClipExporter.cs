using AssetRipper.Core;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using System.IO;
using System.Runtime.Versioning;

namespace AssetRipper.Library.Exporters.Audio
{
	public class AudioClipExporter : BinaryAssetExporter
	{
		private AudioExportFormat AudioFormat { get; }
		public AudioClipExporter(LibraryConfiguration configuration) => AudioFormat = configuration.AudioExportFormat;

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return IsSupportedExportFormat(AudioFormat) && asset is IAudioClip audio && AudioClipDecoder.CanDecode(audio);
		}

		private static bool IsSupportedExportFormat(AudioExportFormat format) => format switch
		{
			AudioExportFormat.Default or AudioExportFormat.PreferWav or AudioExportFormat.PreferMp3 => true,
			_ => false,
		};

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, GetFileExtension((IAudioClip)asset));
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			if (!AudioClipDecoder.TryGetDecodedAudioClipData((IAudioClip)asset, out byte[]? decodedData, out string? fileExtension))
			{
				return false;
			}

			if (AudioFormat == AudioExportFormat.PreferWav || AudioFormat == AudioExportFormat.PreferMp3)
			{
				if (fileExtension == "ogg")
				{
					decodedData = AudioConverter.OggToWav(decodedData);
				}

				if (IsMp3Extension(fileExtension))
				{
					decodedData = AudioConverter.WavToMp3(decodedData);
				}
			}

			if (decodedData.IsNullOrEmpty())
			{
				return false;
			}

			TaskManager.AddTask(File.WriteAllBytesAsync(path, decodedData));
			return true;
		}

		private string GetFileExtension(IAudioClip audioClip)
		{
			string defaultExtension = AudioClipDecoder.GetFileExtension(audioClip);
			if (IsWavExtension(defaultExtension))
			{
				return "wav";
			}

			if (IsMp3Extension(defaultExtension))
			{
				return "mp3";
			}
			else
			{
				return defaultExtension;
			}
		}

		[SupportedOSPlatformGuard("windows")]
		private bool IsMp3Extension(string defaultExtension)
		{
			return AudioFormat == AudioExportFormat.PreferMp3 && OperatingSystem.IsWindows() && (defaultExtension == "ogg" || defaultExtension == "wav");
		}

		private bool IsWavExtension(string defaultExtension)
		{
			return AudioFormat == AudioExportFormat.PreferWav && defaultExtension == "ogg";
		}
	}
}
