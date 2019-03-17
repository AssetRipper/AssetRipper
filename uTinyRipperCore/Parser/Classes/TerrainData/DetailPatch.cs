using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct DetailPatch : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Bounds.Read(reader);
			m_layerIndices = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
			m_numberOfObjects = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(BoundsName, Bounds.ExportYAML(container));
			node.Add(LayerIndicesName, LayerIndices.ExportYAML());
			node.Add(NumberOfObjectsName, NumberOfObjects.ExportYAML());
			return node;
		}

		public IReadOnlyList<byte> LayerIndices => m_layerIndices;
		public IReadOnlyList<byte> NumberOfObjects => m_numberOfObjects;

		public const string BoundsName = "bounds";
		public const string LayerIndicesName = "layerIndices";
		public const string NumberOfObjectsName = "numberOfObjects";

		public AABB Bounds;

		private byte[] m_layerIndices;
		private byte[] m_numberOfObjects;
	}
}
