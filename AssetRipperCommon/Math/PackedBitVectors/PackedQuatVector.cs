using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public class PackedQuatVector : IAssetReadable, IYAMLExportable, IPackedQuatVector
	{
		public const string NumItemsName = "m_NumItems";
		public const string DataName = "m_Data";

		public uint NumItems { get; set; }
		public byte[] Data { get; set; }
		
		public void Read(AssetReader reader)
		{
			NumItems = reader.ReadUInt32();
			Data = reader.ReadByteArray();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer _)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NumItemsName, NumItems);
			node.Add(DataName, Data == null ? YAMLSequenceNode.Empty : Data.ExportYAML());
			return node;
		}

	}
}
