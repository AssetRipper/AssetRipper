using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.OcclusionCullingDatas
{
	public struct OcclusionScene : IAssetReadable, IYAMLExportable
	{
		public OcclusionScene(UnityGUID scene, int renderSize, int portalSize)
		{
			Scene = scene;
			IndexRenderers = 0;
			SizeRenderers = renderSize;
			IndexPortals = 0;
			SizePortals = portalSize;
		}

		public void Read(AssetReader reader)
		{
			IndexRenderers = reader.ReadInt32();
			SizeRenderers = reader.ReadInt32();
			IndexPortals = reader.ReadInt32();
			SizePortals = reader.ReadInt32();
			Scene.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(IndexRenderersName, IndexRenderers);
			node.Add(SizeRenderersName, SizeRenderers);
			node.Add(IndexPortalsName, IndexPortals);
			node.Add(SizePortalsName, SizePortals);
			node.Add(SceneName, Scene.ExportYAML(container));
			return node;
		}

		public int IndexRenderers { get; set; }
		public int SizeRenderers { get; set; }
		public int IndexPortals { get; set; }
		public int SizePortals { get; set; }

		public const string IndexRenderersName = "indexRenderers";
		public const string SizeRenderersName = "sizeRenderers";
		public const string IndexPortalsName = "indexPortals";
		public const string SizePortalsName = "sizePortals";
		public const string SceneName = "scene";

		public UnityGUID Scene;
	}
}