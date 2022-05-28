using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc
{
	public sealed class BitField : IAssetReadable, IYamlExportable
	{
		public BitField() { }

		public BitField(uint bits)
		{
			Bits = bits;
		}

		public static int ToSerializedVersion(UnityVersion version)
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
		private static bool Is16Bits(UnityVersion version) => version.IsLess(2);

		public void Read(AssetReader reader)
		{
			Bits = Is16Bits(reader.Version) ? reader.ReadUInt16() : reader.ReadUInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(BitsName, Bits);
			return node;
		}

		public uint Bits { get; set; }

		public const string BitsName = "m_Bits";
	}
}
