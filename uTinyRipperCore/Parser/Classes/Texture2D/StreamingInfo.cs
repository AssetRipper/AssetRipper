using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.Textures
{
	public struct StreamingInfo : IAssetReadable, IYAMLExportable
	{
		public StreamingInfo(bool _):
			this()
		{
			Path = string.Empty;
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

		public uint Offset { get; private set; }
		public uint Size { get; private set; }
		public string Path { get; private set; }
	}
}
