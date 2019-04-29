using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	public struct VariableBoneCountWeights : IAssetReadable, IYAMLExportable
	{
		public VariableBoneCountWeights(bool _)
		{
			m_data = new uint[0];
		}

		public void Read(AssetReader reader)
		{
			m_data = reader.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(DataName, Data.ExportYAML(true));
			return node;
		}

		public IReadOnlyList<uint> Data => m_data;

		public const string DataName = "m_Data";

		private uint[] m_data;
	}
}
