using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.OcclusionCullingData
{
	public sealed class OcclusionScene : UnityAssetBase, IOcclusionScene
	{
		public override void Read(AssetReader reader)
		{
			IndexRenderers = reader.ReadInt32();
			SizeRenderers = reader.ReadInt32();
			IndexPortals = reader.ReadInt32();
			SizePortals = reader.ReadInt32();
			scene.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(IndexRenderersName, IndexRenderers);
			node.Add(SizeRenderersName, SizeRenderers);
			node.Add(IndexPortalsName, IndexPortals);
			node.Add(SizePortalsName, SizePortals);
			node.Add(SceneName, scene.ExportYAML(container));
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

		public UnityGUID scene = new();

		public UnityGUID Scene
		{
			get { return scene; }
			set { scene = value; }
		}
	}
}
