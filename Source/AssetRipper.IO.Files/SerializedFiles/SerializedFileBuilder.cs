using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.SerializedFiles;

public sealed class SerializedFileBuilder
{
	public FormatVersion Generation { get; set; }
	public UnityVersion Version { get; set; }
	public BuildTarget Platform { get; set; }
	public EndianType EndianType { get; set; }
	public List<FileIdentifier> Dependencies { get; } = [];
	public List<LocalSerializedObjectIdentifier> ScriptTypes { get; } = [];
	public List<ObjectInfo> Objects { get; } = [];
	public List<SerializedType> Types { get; } = [];
	public List<SerializedTypeReference> RefTypes { get; } = [];
	public bool HasTypeTree { get; set; }
	public Utf8String UserInformation { get; set; } = Utf8String.Empty;

	public SerializedFile Build()
	{
		return SerializedFile.FromBuilder(this);
	}
}
