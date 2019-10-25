using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct Face : IAsset
	{
		public Face Convert(IExportContainer container)
		{
			return this;
		}

		public void Read(AssetReader reader)
		{
			V1 = reader.ReadUInt16();
			V2 = reader.ReadUInt16();
			V3 = reader.ReadUInt16();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(V1);
			writer.Write(V2);
			writer.Write(V3);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(V1Name, V1);
			node.Add(V2Name, V2);
			node.Add(V3Name, V3);
			return node;
		}

		public ushort V1 { get; set; }
		public ushort V2 { get; set; }
		public ushort V3 { get; set; }

		public const string V1Name = "v1";
		public const string V2Name = "v2";
		public const string V3Name = "v3";
	}
}
