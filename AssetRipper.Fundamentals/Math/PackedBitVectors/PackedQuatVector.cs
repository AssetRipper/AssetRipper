using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Math.PackedBitVectors
{
	public sealed class PackedQuatVector : IPackedQuatVector
	{
		public const string NumItemsName = "m_NumItems";
		public const string DataName = "m_Data";

		public uint NumItems { get; set; }
		public byte[] Data { get; set; } = Array.Empty<byte>();

		public void Read(AssetReader reader)
		{
			NumItems = reader.ReadUInt32();
			Data = reader.ReadByteArray();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(NumItems);
			writer.Write(Data);
			writer.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer _)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(NumItemsName, NumItems);
			node.Add(DataName, Data == null ? YamlSequenceNode.Empty : Data.ExportYaml());
			return node;
		}
	}
}
