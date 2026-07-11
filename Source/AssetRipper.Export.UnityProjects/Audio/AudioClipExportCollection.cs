using AssetRipper.Assets;
using AssetRipper.Export.Modules.Audio;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.UnityProjects.Audio;

public sealed class AudioClipExportCollection : AudioExportCollection
{
	private readonly string fileExtension;
	public AudioClipExportCollection(AudioClipExporter assetExporter, IAudioClip asset, string fileExtension) : base(assetExporter, asset)
	{
		this.fileExtension = fileExtension;
	}

	protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		if (!TryDecodeAudioData(out byte[]? data, out string? message))
		{
			Logger.Error(LogCategory.Export, message);
			return false;
		}
		else
		{
			fileSystem.File.WriteAllBytes(filePath, data);
			return true;
		}
	}

	private bool TryDecodeAudioData([NotNullWhen(true)] out byte[]? decodedData, [NotNullWhen(false)] out string? message)
	{
		if (AudioClipDecoder.TryDecode(Asset, out decodedData, out string? fileExtension, out message))
		{
			if (fileExtension is "ogg" && this.fileExtension is "wav")
			{
				decodedData = AudioConverter.OggToWav(decodedData);
			}
			return true;
		}
		else
		{
			return false;
		}
	}

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		return fileExtension;
	}
}
