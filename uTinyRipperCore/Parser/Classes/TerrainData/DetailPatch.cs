using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TerrainDatas
{
	public struct DetailPatch : IAsset
	{
		public void Read(AssetReader reader)
		{
			Bounds.Read(reader);
			LayerIndices = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
			NumberOfObjects = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
		}

		public void Write(AssetWriter writer)
		{
			Bounds.Write(writer);
			writer.WriteArray(LayerIndices);
			writer.AlignStream(AlignType.Align4);
			writer.WriteArray(NumberOfObjects);
			writer.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(BoundsName, Bounds.ExportYAML(container));
			node.Add(LayerIndicesName, LayerIndices.ExportYAML());
			node.Add(NumberOfObjectsName, NumberOfObjects.ExportYAML());
			return node;
		}

		public byte[] LayerIndices { get; set; }
		public byte[] NumberOfObjects { get; set; }

		public const string BoundsName = "bounds";
		public const string LayerIndicesName = "layerIndices";
		public const string NumberOfObjectsName = "numberOfObjects";

		public AABB Bounds;
	}
}
