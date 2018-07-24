using System;
using System.Collections.Generic;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.OcclusionCullingDatas;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class OcclusionCullingData : NamedObject
	{
		public OcclusionCullingData(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public OcclusionCullingData(VirtualSerializedFile file):
			this(file.CreateAssetInfo(ClassIDType.OcclusionCullingData))
		{
			Name = nameof(OcclusionCullingData);

			file.AddAsset(this);
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadStaticRenderers(TransferInstructionFlags flags)
		{
			return !flags.IsSerializeGameRelease();
		}

		private static SceneObjectIdentifier CreateObjectID(IExportContainer container, Object asset)
		{
			long lid = 0;
			if (asset != null)
			{
				string id = container.GetExportID(asset);
				lid = long.Parse(id);
			}
			SceneObjectIdentifier soId = new SceneObjectIdentifier(lid, 0);
			return soId;
		}

		public void Initialize(IExportContainer container, byte[] pvsData, UtinyGUID guid,
			IReadOnlyList<PPtr<Renderer>> renderers, IReadOnlyList<PPtr<OcclusionPortal>> portals)
		{
			m_PVSData = pvsData;
			OcclusionScene scene = new OcclusionScene(guid, renderers.Count, portals.Count);
			m_scenes = new OcclusionScene[] { scene };
			SetIDs(container, guid, renderers, portals);
		}

		public void SetIDs(IExportContainer container,
			UtinyGUID guid, IReadOnlyList<PPtr<Renderer>> renderers, IReadOnlyList<PPtr<OcclusionPortal>> portals)
		{
			if(m_staticRenderers.Length == 0 && renderers.Count != 0 || m_portals.Length == 0 && portals.Count != 0)
			{
				int maxRenderer = Scenes.Max(j => j.IndexRenderers);
				OcclusionScene rscene = Scenes.First(t => t.IndexRenderers == maxRenderer);
				m_staticRenderers = new SceneObjectIdentifier[rscene.IndexRenderers + rscene.SizeRenderers];

				int maxPortal = Scenes.Max(j => j.IndexPortals);
				OcclusionScene pscene = Scenes.First(t => t.IndexPortals == maxPortal);
				m_portals = new SceneObjectIdentifier[pscene.IndexPortals + pscene.SizePortals];
			}

			OcclusionScene curScene = Scenes.First(t => t.Scene == guid);
			if(curScene.SizeRenderers != renderers.Count)
			{
				throw new Exception($"Scene renderer count {curScene.SizeRenderers} doesn't match with given {renderers.Count}");
			}
			if (curScene.SizePortals != portals.Count)
			{
				throw new Exception($"Scene portal count {curScene.SizeRenderers} doesn't match with given {renderers.Count}");
			}

			for (int i = 0; i < renderers.Count; i++)
			{
				PPtr<Renderer> prenderer = renderers[i];
				Renderer renderer = prenderer.FindObject(container);
				m_staticRenderers[curScene.IndexRenderers + i] = CreateObjectID(container, renderer);
			}

			m_portals = new SceneObjectIdentifier[portals.Count];
			for (int i = 0; i < portals.Count; i++)
			{
				PPtr<OcclusionPortal> pportal = portals[i];
				OcclusionPortal portal = pportal.FindObject(container);
				m_portals[i] = CreateObjectID(container, portal);
			}
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_PVSData = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);

			m_scenes = stream.ReadArray<OcclusionScene>();
			if (IsReadStaticRenderers(stream.Flags))
			{
				m_staticRenderers = stream.ReadArray<SceneObjectIdentifier>();
				m_portals = stream.ReadArray<SceneObjectIdentifier>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_PVSData", PVSData.ExportYAML());
			node.Add("m_Scenes", Scenes.ExportYAML(container));
			node.Add("m_StaticRenderers", StaticRenderers.ExportYAML(container));
			node.Add("m_Portals", Portals.ExportYAML(container));
			return node;
		}

		public override string ExportName => $"{OcclusionCullingSettings.SceneExportFolder}/{base.ExportName}";

		public IReadOnlyList<byte> PVSData => m_PVSData;
		public IReadOnlyList<OcclusionScene> Scenes => m_scenes;
		public IReadOnlyList<SceneObjectIdentifier> StaticRenderers => m_staticRenderers;
		public IReadOnlyList<SceneObjectIdentifier> Portals => m_portals;
		
		private byte[] m_PVSData;
		private OcclusionScene[] m_scenes;
		private SceneObjectIdentifier[] m_staticRenderers = new SceneObjectIdentifier[0];
		private SceneObjectIdentifier[] m_portals = new SceneObjectIdentifier[0];
	}
}
