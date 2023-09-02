using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.SerializedFiles;

public sealed class SerializedFileBuilder
{
	public FormatVersion Generation { get; set; }
	public UnityVersion Version { get; set; }
	public BuildTarget Platform { get; set; }
	public EndianType EndianType { get; set; }
	public List<FileIdentifier> Dependencies { get; } = new();
	public List<ObjectInfo> Objects { get; } = new();
	public List<SerializedType> Types { get; } = new();
	public List<SerializedTypeReference> RefTypes { get; } = new();
	public bool HasTypeTree { get; set; }

	public SerializedFile Build()
	{
		return SerializedFile.FromBuilder(this);
	}
}
