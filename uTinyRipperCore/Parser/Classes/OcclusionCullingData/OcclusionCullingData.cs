using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.OcclusionCullingDatas;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class OcclusionCullingData : NamedObject
	{
		public OcclusionCullingData(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private OcclusionCullingData(AssetInfo assetInfo, OcclusionCullingSettings cullingSetting) :
			base(assetInfo, true)
		{
			Name = nameof(OcclusionCullingData);
		}

		public static OcclusionCullingData CreateVirtualInstance(VirtualSerializedFile virtualFile, OcclusionCullingSettings cullingSetting)
		{
			return virtualFile.CreateAsset((assetInfo) => new OcclusionCullingData(assetInfo, cullingSetting));
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadStaticRenderers(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}

		public void Initialize(IExportContainer container, OcclusionCullingSettings cullingSetting)
		{
			m_PVSData = (byte[])cullingSetting.PVSData;
			int renderCount = cullingSetting.StaticRenderers.Count;
			int portalCount = cullingSetting.Portals.Count;
			OcclusionScene scene = new OcclusionScene(cullingSetting.SceneGUID, renderCount, portalCount);
			m_scenes = new OcclusionScene[] { scene };

			m_staticRenderers = new SceneObjectIdentifier[scene.SizeRenderers];
			m_portals = new SceneObjectIdentifier[scene.SizePortals];
			SetIDs(container, cullingSetting, scene);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_PVSData = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);

			m_scenes = reader.ReadAssetArray<OcclusionScene>();
			if (IsReadStaticRenderers(reader.Flags))
			{
				m_staticRenderers = reader.ReadAssetArray<SceneObjectIdentifier>();
				m_portals = reader.ReadAssetArray<SceneObjectIdentifier>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_PVSData", PVSData.ExportYAML());
			node.Add("m_Scenes", Scenes.ExportYAML(container));

			SetExportData(container);
			node.Add("m_StaticRenderers", StaticRenderers.ExportYAML(container));
			node.Add("m_Portals", Portals.ExportYAML(container));
			return node;
		}

		private void SetExportData(IExportContainer container)
		{
			// if < 3.0.0 this asset doesn't exist

			// 3.0.0 to 5.5.0 this asset is created by culling settings so it has set data already
			if(OcclusionCullingSettings.IsReadPVSData(container.Version))
			{
				return;
			}

			// if >= 5.5.0 and !Release this asset containts renderer data
			if (IsReadStaticRenderers(container.Flags))
			{
				return;
			}

			// if >= 5.5.0 and Release this asset doesn't containt renderers data so we need to create it
			List<OcclusionCullingSettings> cullingSettings = new List<OcclusionCullingSettings>();
			foreach (Object asset in File.Collection.FetchAssets())
			{
				if (asset.ClassID == ClassIDType.OcclusionCullingSettings)
				{
					OcclusionCullingSettings cullingSetting = (OcclusionCullingSettings)asset;
					if (cullingSetting.OcclusionCullingData.IsAsset(cullingSetting.File, this))
					{
						cullingSettings.Add(cullingSetting);
					}
				}
			}

			int maxRenderer = Scenes.Max(j => j.IndexRenderers + j.SizeRenderers);
			m_staticRenderers = new SceneObjectIdentifier[maxRenderer];
			int maxPortal = Scenes.Max(j => j.IndexPortals + j.SizePortals);
			m_portals = new SceneObjectIdentifier[maxPortal];

			foreach(OcclusionCullingSettings cullingSetting in cullingSettings)
			{
				int sceneIndex = Scenes.IndexOf(t => t.Scene == cullingSetting.SceneGUID);
				if (sceneIndex == -1)
				{
					Logger.Log(LogType.Error, LogCategory.Export, $"Unable to find scene data with GUID {cullingSetting.SceneGUID} in {ValidName}");
					continue;
				}

				OcclusionScene scene = Scenes[sceneIndex];
				if (scene.SizeRenderers != cullingSetting.StaticRenderers.Count)
				{
					throw new Exception($"Scene renderer count {scene.SizeRenderers} doesn't match with given {cullingSetting.StaticRenderers.Count}");
				}
				if (scene.SizePortals != cullingSetting.Portals.Count)
				{
					throw new Exception($"Scene portal count {scene.SizePortals} doesn't match with given {cullingSetting.Portals.Count}");
				}
				SetIDs(container, cullingSetting, scene);
			}
		}

		private void SetIDs(IExportContainer container, OcclusionCullingSettings cullingSetting, OcclusionScene scene)
		{
			for (int i = 0; i < cullingSetting.StaticRenderers.Count; i++)
			{
				PPtr<Renderer> prenderer = cullingSetting.StaticRenderers[i];
				Renderer renderer = prenderer.FindAsset(cullingSetting.File);
				m_staticRenderers[scene.IndexRenderers + i] = CreateObjectID(container, renderer);
			}

			for (int i = 0; i < cullingSetting.Portals.Count; i++)
			{
				PPtr<OcclusionPortal> pportal = cullingSetting.Portals[i];
				OcclusionPortal portal = pportal.FindAsset(cullingSetting.File);
				m_portals[scene.IndexPortals + i] = CreateObjectID(container, portal);
			}
		}

		private static SceneObjectIdentifier CreateObjectID(IExportContainer container, Object asset)
		{
			long lid = asset == null ? 0 : container.GetExportID(asset);
			SceneObjectIdentifier soId = new SceneObjectIdentifier(lid, 0);
			return soId;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

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
