using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct FixedBitset : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_data = reader.ReadUInt32Array();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("data", Data.ExportYAML(true));
			return node;
		}

		public IReadOnlyList<uint> Data => m_data;

		private uint[] m_data;
	}
}
