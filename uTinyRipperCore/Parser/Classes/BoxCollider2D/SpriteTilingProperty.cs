using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.BoxCollider2Ds
{
	public struct SpriteTilingProperty : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Border.Read(reader);
			Pivot.Read(reader);
			OldSize.Read(reader);
			NewSize.Read(reader);
			AdaptiveTilingThreshold = reader.ReadSingle();
			DrawMode = reader.ReadInt32();
			AdaptiveTiling = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("border", Border.ExportYAML(container));
			node.Add("pivot", Pivot.ExportYAML(container));
			node.Add("oldSize", OldSize.ExportYAML(container));
			node.Add("newSize", NewSize.ExportYAML(container));
			node.Add("adaptiveTilingThreshold", AdaptiveTilingThreshold);
			node.Add("drawMode", DrawMode);
			node.Add("adaptiveTiling", AdaptiveTiling);
			return node;
		}

		public float AdaptiveTilingThreshold { get; private set; }
		public int DrawMode { get; private set; }
		public bool AdaptiveTiling { get; private set; }

		public Vector4f Border;
		public Vector2f Pivot;
		public Vector2f OldSize;
		public Vector2f NewSize;
	}
}
