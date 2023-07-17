using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Audio
{
	public sealed class AudioClipExporter : BinaryAssetExporter
	{
		public AudioExportFormat AudioFormat { get; }
		public AudioClipExporter(LibraryConfiguration configuration) => AudioFormat = configuration.AudioExportFormat;

		public static bool IsSupportedExportFormat(AudioExportFormat format) => format switch
		{
			AudioExportFormat.Default or AudioExportFormat.PreferWav => true,
			_ => false,
		};

		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IAudioClip audio && AudioClipDecoder.CanDecode(audio))
			{
				exportCollection = new AudioClipExportCollection(this, audio);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
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

			File.WriteAllBytes(path, decodedData);
			return true;
		}
	}
}
