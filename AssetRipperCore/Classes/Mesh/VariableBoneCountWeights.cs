using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class VariableBoneCountWeights : IVariableBoneCountWeights
	{
		public void Read(AssetReader reader)
		{
			Data = reader.ReadUInt32Array();
		}

		public void Write(AssetWriter writer)
		{
			Data.Write(writer);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(DataName, Data.ExportYaml(true));
			return node;
		}

		public uint[] Data { get; set; } = Array.Empty<uint>();

		public const string DataName = "m_Data";
	}
}
