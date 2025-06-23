using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles;

public sealed record class DirectoryInfo<T> : IEndianReadable<DirectoryInfo<T>>, IEndianWritable where T : Node, IEndianReadable<T>
{
	public static DirectoryInfo<T> Read(EndianReader reader)
	{
		return new()
		{
			Nodes = reader.ReadEndianArray<T>()
		};
	}

	public void Write(EndianWriter writer)
	{
		writer.WriteEndianArray(Nodes);
	}

	public T[] Nodes { get; set; } = Array.Empty<T>();
}
