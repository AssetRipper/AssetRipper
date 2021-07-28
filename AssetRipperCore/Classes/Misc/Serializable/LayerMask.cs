using AssetRipper.Project;
using AssetRipper.Layout.Classes.Misc.Serializable;
using AssetRipper.Classes.Utils.Extensions;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;

namespace AssetRipper.Classes.Misc.Serializable
{
	public struct LayerMask : IAsset
	{
		public void Read(AssetReader reader)
		{
			LayerMaskLayout layout = reader.Layout.Serialized.LayerMask;
			Bits = layout.Is32Bits ? reader.ReadUInt32() : reader.ReadUInt16();
		}

		public void Write(AssetWriter writer)
		{
			LayerMaskLayout layout = writer.Layout.Serialized.LayerMask;
			if (layout.Is32Bits)
			{
				writer.Write(Bits);
			}
			else
			{
				writer.Write((ushort)Bits);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			LayerMaskLayout layout = container.ExportLayout.Serialized.LayerMask;
			node.AddSerializedVersion(layout.Version);
			node.Add(layout.BitsName, Bits);
			return node;
		}

		public uint Bits { get; set; }
	}
}
