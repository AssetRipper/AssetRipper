using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public struct LayerMask : IAsset
	{
		private static int GetSerializedVersion(Version version)
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
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(BitsName, Bits);
			return node;
		}

		public uint Bits { get; private set; }

		public const string BitsName = "m_Bits";
	}
}
