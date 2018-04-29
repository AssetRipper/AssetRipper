using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.OcclusionCullingDatas
{
	public struct OcclusionScene : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			IndexRenderers = stream.ReadInt32();
			SizeRenderers = stream.ReadInt32();
			IndexPortals = stream.ReadInt32();
			SizePortals = stream.ReadInt32();
			Scene.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("indexRenderers", IndexRenderers);
			node.Add("sizeRenderers", SizeRenderers);
			node.Add("indexPortals", IndexPortals);
			node.Add("sizePortals", SizePortals);
			node.Add("scene", Scene.ExportYAML(exporter));
			return node;
		}

		public int IndexRenderers { get; private set; }
		public int SizeRenderers { get; private set; }
		public int IndexPortals { get; private set; }
		public int SizePortals { get; private set; }

		public UtinyGUID Scene;
	}
}