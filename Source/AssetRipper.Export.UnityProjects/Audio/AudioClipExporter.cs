using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.Modules.Audio;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.UnityProjects.Audio;

public sealed class AudioClipExporter : BinaryAssetExporter
{
	public AudioExportFormat AudioFormat { get; }
	public AudioClipExporter(FullConfiguration configuration) => AudioFormat = configuration.ExportSettings.AudioExportFormat;

	public static bool IsSupportedExportFormat(AudioExportFormat format) => format switch
	{
		AudioExportFormat.Default or AudioExportFormat.PreferWav => true,
		_ => false,
	};

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IAudioClip audio)
		{
			if (!AudioClipDecoder.TryDecode(audio, out byte[]? decodedData, out string? fileExtension, out string? message))
			{
				Logger.Warning(LogCategory.Export, message);
			}
			else if (decodedData.Length == 0)
			{
				Logger.Warning(LogCategory.Export, $"Decoded audio data is empty for '{audio.Name}'");
			}
			else
			{
				if (AudioFormat == AudioExportFormat.PreferWav && fileExtension == "ogg")
				{
					exportCollection = new AudioClipExportCollection(this, audio, "wav");
				}
				else
				{
					exportCollection = new AudioClipExportCollection(this, audio, fileExtension);
				}

				return true;
			}
		}

		exportCollection = null;
		return false;
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		throw new NotSupportedException();
	}
}
