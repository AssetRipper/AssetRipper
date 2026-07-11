using AssetRipper.Export.Modules.Audio;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.PrimaryContent.Audio;

public sealed class AudioExportCollection : SingleExportCollection<IAudioClip>
{
	private readonly string extension;

	private AudioExportCollection(AudioContentExtractor contentExtractor, IAudioClip asset, string extension) : base(contentExtractor, asset)
	{
		this.extension = extension;
	}

	public static bool TryCreate(AudioContentExtractor contentExtractor, IAudioClip asset, [NotNullWhen(true)] out AudioExportCollection? exportCollection)
	{
		// We decode twice to avoid storing the decoded data in memory.

		if (!AudioClipDecoder.TryDecode(asset, out byte[]? data, out string? extension, out string? message))
		{
			Logger.Log(LogType.Warning, LogCategory.Export, message);
			exportCollection = null;
			return false;
		}
		else if (data.Length == 0)
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"AudioClip '{asset.Name}' has no audio data.");
			exportCollection = null;
			return false;
		}
		else
		{
			exportCollection = new AudioExportCollection(contentExtractor, asset, extension);
			return true;
		}
	}

	protected override bool ExportInner(string filePath, string dirPath, FileSystem fileSystem)
	{
		if (AudioClipDecoder.TryDecode(Asset, out byte[]? data, out _, out _))
		{
			fileSystem.File.WriteAllBytes(filePath, data);
			return true;
		}
		else
		{
			return false;
		}
	}

	protected override string ExportExtension => extension;
}
