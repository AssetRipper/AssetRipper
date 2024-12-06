using AssetRipper.Export.Modules.Audio;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.PrimaryContent.Audio;

public sealed class AudioExportCollection : SingleExportCollection<IAudioClip>
{
	private byte[] data;
	private readonly string extension;

	private AudioExportCollection(AudioContentExtractor contentExtractor, IAudioClip asset, byte[] data, string extension) : base(contentExtractor, asset)
	{
		this.data = data;
		this.extension = extension;
	}

	public static bool TryCreate(AudioContentExtractor contentExtractor, IAudioClip asset, [NotNullWhen(true)] out AudioExportCollection? exportCollection)
	{
		if (AudioClipDecoder.TryDecode(asset, out byte[]? data, out string? extension, out string? message))
		{
			exportCollection = new AudioExportCollection(contentExtractor, asset, data, extension);
			return true;
		}
		else
		{
			Logger.Log(LogType.Warning, LogCategory.Export, message);
			exportCollection = null;
			return false;
		}
	}

	protected override bool ExportInner(string filePath, string dirPath, FileSystem fileSystem)
	{
		if (data.Length > 0)
		{
			fileSystem.File.WriteAllBytes(filePath, data);
			data = []; // Export is only called once, so we can clear the data.
			return true;
		}
		else
		{
			return false;
		}
	}

	protected override string ExportExtension => extension;
}
