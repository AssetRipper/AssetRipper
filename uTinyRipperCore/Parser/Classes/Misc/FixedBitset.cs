using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes;

namespace uTinyRipper.Classes.Misc
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
