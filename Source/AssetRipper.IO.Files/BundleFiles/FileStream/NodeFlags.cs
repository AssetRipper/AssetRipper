namespace AssetRipper.IO.Files.BundleFiles.FileStream;

[Flags]
public enum NodeFlags
{
	Default = 0,
	Directory = 1,
	Deleted = 2,
	SerializedFile = 4,
}
