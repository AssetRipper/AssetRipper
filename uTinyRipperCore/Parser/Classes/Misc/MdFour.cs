using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct MdFour : IAsset
	{
		public static int ToSerializedVersion(Version version)
		{
			// unknown version
			return 2;
		}

		public void Read(AssetReader reader)
		{
			Md4Hash = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
		}

		public void Write(AssetWriter writer)
		{
			Md4Hash.Write(writer);
			writer.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(Md4HashName, Md4Hash.ExportYAML());
			return node;
		}

		public byte[] Md4Hash { get; set; }

		public const string Md4HashName = "md4 hash";
	}
}
