using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.ResourceFiles;

public sealed class ResourceFile : FileBase
{
	public ResourceFile(SmartStream stream, string filePath, string name)
	{
		Stream = stream.CreateReference();
		FilePath = filePath;
		Name = name;
	}

	public ResourceFile(byte[] data, string filePath, string name, bool writable = true)
	{
		Stream = SmartStream.CreateMemory(data, 0, data.Length, writable);
		FilePath = filePath;
		Name = name;
	}

	public ResourceFile(string filePath, string name, FileSystem fileSystem)
	{
		Stream = SmartStream.OpenRead(filePath, fileSystem);
		FilePath = filePath;
		Name = name;
	}

	public bool IsDefaultResourceFile() => IsDefaultResourceFile(Name);

	public static bool IsDefaultResourceFile(string fileName)
	{
		string extension = Path.GetExtension(fileName).ToLowerInvariant();
		return extension is ResourceFileExtension or StreamingFileExtension;
	}

	public override string ToString() => Name;

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		Stream.Dispose();
	}

	public override void Read(SmartStream stream)
	{
		throw new NotSupportedException();
	}

	public override void Write(Stream stream)
	{
		Stream.CopyTo(stream);
	}

	public override byte[] ToByteArray()
	{
		return Stream.ToArray();
	}

	public SmartStream Stream { get; }

	public const string ResourceFileExtension = ".resource";
	public const string StreamingFileExtension = ".ress";
}
