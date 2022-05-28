using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc.Serializable
{
	public sealed class LayerMask : IAsset
	{
		public void Read(AssetReader reader)
		{
			Bits = Is32Bits(reader.Version) ? reader.ReadUInt32() : reader.ReadUInt16();
		}

		public void Write(AssetWriter writer)
		{
			if (Is32Bits(writer.Version))
			{
				writer.Write(Bits);
			}
			else
			{
				writer.Write((ushort)Bits);
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(BitsName, Bits);
			return node;
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(2))
			{
				// Bits size has been changed to 32
				return 2;
			}
			else
			{
				return 1;
			}
		}

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public bool Is32Bits(UnityVersion version) => version.IsGreaterEqual(2);

		public uint Bits { get; set; }

		public const string BitsName = "m_Bits";
	}
}
