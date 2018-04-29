using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.OcclusionCullingDatas;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class OcclusionCullingData : NamedObject
	{
		public OcclusionCullingData(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// NoTarget
		/// </summary>
		public static bool IsReadStaticRenderers(Platform platform)
		{
			return platform == Platform.NoTarget;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_PVSData = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);

			m_scenes = stream.ReadArray<OcclusionScene>();
			if (IsReadStaticRenderers(stream.Platform))
			{
				m_staticRenderers = stream.ReadArray<SceneObjectIdentifier>();
				m_portals = stream.ReadArray<SceneObjectIdentifier>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_PVSData", PVSData.ExportYAML());
			node.Add("m_Scenes", Scenes.ExportYAML(exporter));
			node.Add("m_StaticRenderers", IsReadStaticRenderers(exporter.Platform) ? StaticRenderers.ExportYAML(exporter) : YAMLSequenceNode.Empty);
			node.Add("m_Portals", IsReadStaticRenderers(exporter.Platform) ? Portals.ExportYAML(exporter) : YAMLSequenceNode.Empty);
			return node;
		}

		public IReadOnlyList<byte> PVSData => m_PVSData;
		public IReadOnlyList<OcclusionScene> Scenes => m_scenes;
		public IReadOnlyList<SceneObjectIdentifier> StaticRenderers => m_staticRenderers;
		public IReadOnlyList<SceneObjectIdentifier> Portals => m_portals;
		
		private byte[] m_PVSData;
		private OcclusionScene[] m_scenes;
		private SceneObjectIdentifier[] m_staticRenderers;
		private SceneObjectIdentifier[] m_portals;
	}
}
