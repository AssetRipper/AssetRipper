using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper.Classes.OcclusionCullingDatas;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Converters;
using uTinyRipper;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public sealed class OcclusionCullingData : NamedObject
	{
		public OcclusionCullingData(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private OcclusionCullingData(AssetLayout layout, AssetInfo assetInfo) :
			base(layout)
		{
			AssetInfo = assetInfo;
			Name = nameof(OcclusionCullingData);
		}

		public static OcclusionCullingData CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new OcclusionCullingData(virtualFile.Layout, assetInfo));
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasStaticRenderers(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}

		public void Initialize(IExportContainer container, OcclusionCullingSettings cullingSetting)
		{
			PVSData = (byte[])cullingSetting.PVSData;
			int renderCount = cullingSetting.StaticRenderers.Length;
			int portalCount = cullingSetting.Portals.Length;
			OcclusionScene scene = new OcclusionScene(cullingSetting.SceneGUID, renderCount, portalCount);
			Scenes = new OcclusionScene[] { scene };

			StaticRenderers = new SceneObjectIdentifier[scene.SizeRenderers];
			Portals = new SceneObjectIdentifier[scene.SizePortals];
			SetIDs(container, cullingSetting, scene);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			PVSData = reader.ReadByteArray();
			reader.AlignStream();

			Scenes = reader.ReadAssetArray<OcclusionScene>();
			if (HasStaticRenderers(reader.Flags))
			{
				StaticRenderers = reader.ReadAssetArray<SceneObjectIdentifier>();
				Portals = reader.ReadAssetArray<SceneObjectIdentifier>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(PVSDataName, PVSData.ExportYAML());
			node.Add(ScenesName, Scenes.ExportYAML(container));
			SetExportData(container);
			node.Add(StaticRenderersName, StaticRenderers.ExportYAML(container));
			node.Add(PortalsName, Portals.ExportYAML(container));
			return node;
		}

		private void SetExportData(IExportContainer container)
		{
			// if < 3.0.0 this asset doesn't exist

			// 3.0.0 to 5.5.0 this asset is created by culling settings so it has set data already
			if(OcclusionCullingSettings.HasReadPVSData(container.Version))
			{
				return;
			}

			// if >= 5.5.0 and !Release this asset containts renderer data
			if (HasStaticRenderers(container.Flags))
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
			StaticRenderers = new SceneObjectIdentifier[maxRenderer];
			int maxPortal = Scenes.Max(j => j.IndexPortals + j.SizePortals);
			Portals = new SceneObjectIdentifier[maxPortal];

			foreach(OcclusionCullingSettings cullingSetting in cullingSettings)
			{
				int sceneIndex = Scenes.IndexOf(t => t.Scene == cullingSetting.SceneGUID);
				if (sceneIndex == -1)
				{
					Logger.Log(LogType.Error, LogCategory.Export, $"Unable to find scene data with GUID {cullingSetting.SceneGUID} in {ValidName}");
					continue;
				}

				OcclusionScene scene = Scenes[sceneIndex];
				if (scene.SizeRenderers != cullingSetting.StaticRenderers.Length)
				{
					throw new Exception($"Scene renderer count {scene.SizeRenderers} doesn't match with given {cullingSetting.StaticRenderers.Length}");
				}
				if (scene.SizePortals != cullingSetting.Portals.Length)
				{
					throw new Exception($"Scene portal count {scene.SizePortals} doesn't match with given {cullingSetting.Portals.Length}");
				}
				SetIDs(container, cullingSetting, scene);
			}
		}

		private void SetIDs(IExportContainer container, OcclusionCullingSettings cullingSetting, OcclusionScene scene)
		{
			for (int i = 0; i < cullingSetting.StaticRenderers.Length; i++)
			{
				PPtr<Renderer> prenderer = cullingSetting.StaticRenderers[i];
				Renderer renderer = prenderer.FindAsset(cullingSetting.File);
				StaticRenderers[scene.IndexRenderers + i] = CreateObjectID(container, renderer);
			}

			for (int i = 0; i < cullingSetting.Portals.Length; i++)
			{
				PPtr<OcclusionPortal> pportal = cullingSetting.Portals[i];
				OcclusionPortal portal = pportal.FindAsset(cullingSetting.File);
				Portals[scene.IndexPortals + i] = CreateObjectID(container, portal);
			}
		}

		private static SceneObjectIdentifier CreateObjectID(IExportContainer container, Object asset)
		{
			long lid = asset == null ? 0 : container.GetExportID(asset);
			SceneObjectIdentifier soId = new SceneObjectIdentifier(lid, 0);
			return soId;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

		public byte[] PVSData { get; set; }
		public OcclusionScene[] Scenes { get; set; }
		public SceneObjectIdentifier[] StaticRenderers { get; set; } = Array.Empty<SceneObjectIdentifier>();
		public SceneObjectIdentifier[] Portals { get; set; } = Array.Empty<SceneObjectIdentifier>();

		public const string PVSDataName = "m_PVSData";
		public const string ScenesName = "m_Scenes";
		public const string StaticRenderersName = "m_StaticRenderers";
		public const string PortalsName = "m_Portals";
	}
}
