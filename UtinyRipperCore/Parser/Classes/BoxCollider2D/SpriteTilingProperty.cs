using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.BoxCollider2Ds
{
	public struct SpriteTilingProperty : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Border.Read(stream);
			Pivot.Read(stream);
			OldSize.Read(stream);
			NewSize.Read(stream);
			AdaptiveTilingThreshold = stream.ReadSingle();
			DrawMode = stream.ReadInt32();
			AdaptiveTiling = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
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
