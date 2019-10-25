using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct BitField : IAssetReadable, IYAMLExportable
	{
		public BitField(uint bits)
		{
			Bits = bits;
		}

		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		private static bool Is16Bits(Version version)
		{
			return version.IsLess(2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(2))
			{
				return 2;
			}
			return 1;
		}
		
		public void Read(AssetReader reader)
		{
			Bits = Is16Bits(reader.Version) ? reader.ReadUInt16() : reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Bits", Bits);
			return node;
		}

		public uint Bits { get; private set; }
	}
}
