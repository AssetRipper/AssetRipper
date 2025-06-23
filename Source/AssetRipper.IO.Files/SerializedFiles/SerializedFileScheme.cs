using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.SerializedFiles;

public sealed class SerializedFileScheme : Scheme<SerializedFile>
{
	public static SerializedFileScheme Default { get; } = new();

	public override bool CanRead(SmartStream stream)
	{
		return SerializedFile.IsSerializedFile(stream);
	}
}
