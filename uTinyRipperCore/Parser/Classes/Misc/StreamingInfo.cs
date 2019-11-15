using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct StreamingInfo : IAsset
	{
		public StreamingInfo(bool _)
		{
			Offset = 0;
			Size = 0;
			Path = string.Empty;
		}

		public StreamingInfo(Version version)
		{
			Offset = 0;
			Size = 0;
			Path = string.Empty;
		}

		public bool CheckIntegrity(ISerializedFile file)
		{
			if (!IsSet)
			{
				return true;
			}
			return file.Collection.FindResourceFile(Path) != null;
		}

		public byte[] GetContent(ISerializedFile file)
		{
			IResourceFile res = file.Collection.FindResourceFile(Path);
			if (res == null)
			{
				return null;
			}

			byte[] data = new byte[Size];
			res.Stream.Position = Offset;
			res.Stream.ReadBuffer(data, 0, data.Length);
			return data;
		}

		public void Read(AssetReader reader)
		{
			Offset = reader.ReadUInt32();
			Size = reader.ReadUInt32();
			Path = reader.ReadString();
		}

		public void Read(AssetReader reader, string path)
		{
			Size = reader.ReadUInt32();
			Offset = reader.ReadUInt32();
			Path = path;
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Offset);
			writer.Write(Size);
			writer.Write(Path);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(OffsetName, Offset);
			node.Add(SizeName, Size);
			node.Add(PathName, Path);
			return node;
		}

		public const string OffsetName = "offset";
		public const string SizeName = "size";
		public const string PathName = "path";

		public bool IsSet => Path.Length > 0;

		public uint Offset { get; set; }
		public uint Size { get; set; }
		public string Path { get; set; }
	}
}
