using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Hand : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_handBoneIndex = reader.ReadInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_HandBoneIndex", m_handBoneIndex.ExportYAML(true));
			return node;
		}

		public IReadOnlyList<int> HandBoneIndex => m_handBoneIndex;

		private int[] m_handBoneIndex;
	}
}
