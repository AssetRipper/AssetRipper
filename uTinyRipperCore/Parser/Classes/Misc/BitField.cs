using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	public struct BitField : IAssetReadable, IYAMLExportable
	{
		public BitField(uint bits)
		{
			Bits = bits;
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		private static bool Is16Bits(Version version) => version.IsLess(2);

		public void Read(AssetReader reader)
		{
			Bits = Is16Bits(reader.Version) ? reader.ReadUInt16() : reader.ReadUInt32();
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
