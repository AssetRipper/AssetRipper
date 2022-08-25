using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.BundleFiles
{
	public sealed record class DirectoryInfo<T> : IEndianReadable, IEndianWritable where T : Node, new()
	{
		public void Read(EndianReader reader)
		{
			Nodes = reader.ReadEndianArray<T>();
		}

		public void Write(EndianWriter writer)
		{
			writer.WriteEndianArray(Nodes);
		}

		public T[] Nodes { get; set; } = Array.Empty<T>();
	}
}
