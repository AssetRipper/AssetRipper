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

namespace AssetRipper.Library.Exporters.Audio
{
	public sealed class AudioClipExporter : BinaryAssetExporter
	{
		public AudioExportFormat AudioFormat { get; }
		public AudioClipExporter(LibraryConfiguration configuration) => AudioFormat = configuration.AudioExportFormat;

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IAudioClip audio && AudioClipDecoder.CanDecode(audio);
		}

		public static bool IsSupportedExportFormat(AudioExportFormat format) => format switch
		{
			AudioExportFormat.Default or AudioExportFormat.PreferWav => true,
			_ => false,
		};

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AudioClipExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			if (!AudioClipDecoder.TryGetDecodedAudioClipData((IAudioClip)asset, out byte[]? decodedData, out string? fileExtension))
			{
				return false;
			}

			if (AudioFormat == AudioExportFormat.PreferWav && fileExtension == "ogg")
			{
				decodedData = AudioConverter.OggToWav(decodedData);
			}

			if (decodedData.IsNullOrEmpty())
			{
				return false;
			}

			TaskManager.AddTask(File.WriteAllBytesAsync(path, decodedData));
			return true;
		}
	}
}
