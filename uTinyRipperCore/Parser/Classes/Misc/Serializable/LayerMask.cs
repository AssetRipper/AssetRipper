using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public struct LayerMask : IAsset
	{
		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}

		public void Read(AssetReader reader)
		{
			Bits = reader.ReadUInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Bits);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(BitsName, Bits);
			return node;
		}

		public uint Bits { get; set; }

		public const string BitsName = "m_Bits";
	}
}
