using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_29;
using AssetRipper.SourceGenerated.Classes.ClassID_363;
using AssetRipper.SourceGenerated.Classes.ClassID_41;
using AssetRipper.SourceGenerated.Subclasses.GUID;
using AssetRipper.SourceGenerated.Subclasses.OcclusionScene;
using AssetRipper.SourceGenerated.Subclasses.PPtr_OcclusionPortal_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Renderer_;
using AssetRipper.SourceGenerated.Subclasses.SceneObjectIdentifier;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class OcclusionCullingDataExtensions
	{
		public static void Initialize(this IOcclusionCullingData occlusionCullingData, IExportContainer container, IOcclusionCullingSettings cullingSetting)
		{
			occlusionCullingData.PVSData_C363 = cullingSetting.PVSData_C29 ?? Array.Empty<byte>();
			int renderCount = cullingSetting.StaticRenderers_C29?.Count ?? 0;
			int portalCount = cullingSetting.Portals_C29?.Count ?? 0;
			occlusionCullingData.Scenes_C363.InitializeList(1);
			OcclusionScene scene = occlusionCullingData.Scenes_C363[0];
			scene.Initialize(cullingSetting.SceneGUID_C29, renderCount, portalCount);

			occlusionCullingData.StaticRenderers_C363.InitializeList(scene.SizeRenderers);
			occlusionCullingData.Portals_C363.InitializeList(scene.SizePortals);
			occlusionCullingData.SetIDs(container, cullingSetting, scene);
		}

		private static void InitializeList<T>(this AssetList<T> list, int size) where T : notnull, new()
		{
			list.Clear();
			list.Capacity = size;
			for (int i = 0; i < size; i++)
			{
				list.AddNew();
			}
		}

		public static void Initialize(this OcclusionScene occlusionScene, GUID? scene, int renderSize, int portalSize)
		{
			if (scene is not null)
			{
				occlusionScene.Scene.CopyValues(scene);
			}
			occlusionScene.IndexRenderers = 0;
			occlusionScene.SizeRenderers = renderSize;
			occlusionScene.IndexPortals = 0;
			occlusionScene.SizePortals = portalSize;
		}

		public static void SetExportData(this IOcclusionCullingData occlusionCullingData, IExportContainer container)
		{
			// if < 3.0.0 this asset doesn't exist

			// 3.0.0 to 5.5.0 this asset is created by culling settings so it has set data already
			if (container.Version.IsLess(5, 5))
			{
				return;
			}

			// if >= 5.5.0 and !Release this asset containts renderer data
			if (!container.Flags.IsRelease())
			{
				return;
			}

			// if >= 5.5.0 and Release this asset doesn't containt renderers data so we need to create it
			List<IOcclusionCullingSettings> cullingSettings = new List<IOcclusionCullingSettings>();
			foreach (IUnityObjectBase asset in occlusionCullingData.SerializedFile.Collection.FetchAssets())
			{
				if (asset is IOcclusionCullingSettings cullingSetting 
					&& cullingSetting.Has_OcclusionCullingData_C29() 
					&& cullingSetting.OcclusionCullingData_C29.IsAsset(cullingSetting.SerializedFile, occlusionCullingData))
				{
					cullingSettings.Add(cullingSetting);
				}
			}

			int maxRenderer = occlusionCullingData.Scenes_C363.Max(j => j.IndexRenderers + j.SizeRenderers);
			occlusionCullingData.StaticRenderers_C363.InitializeList(maxRenderer);
			int maxPortal = occlusionCullingData.Scenes_C363.Max(j => j.IndexPortals + j.SizePortals);
			occlusionCullingData.Portals_C363.InitializeList(maxPortal);

			foreach (IOcclusionCullingSettings cullingSetting in cullingSettings)
			{
				if (!cullingSetting.Has_SceneGUID_C29())
				{
					continue;
				}

				int sceneIndex = occlusionCullingData.Scenes_C363.IndexOf(t => t.Scene == cullingSetting.SceneGUID_C29);
				if (sceneIndex == -1)
				{
					Logger.Log(LogType.Error, LogCategory.Export, $"Unable to find scene data with GUID {cullingSetting.SceneGUID_C29} in {occlusionCullingData.GetNameNotEmpty()}");
					continue;
				}

				IOcclusionScene scene = occlusionCullingData.Scenes_C363[sceneIndex];
				if (scene.SizeRenderers != cullingSetting.StaticRenderers_C29.Count)
				{
					throw new Exception($"Scene renderer count {scene.SizeRenderers} doesn't match with given {cullingSetting.StaticRenderers_C29.Count}");
				}
				if (scene.SizePortals != cullingSetting.Portals_C29.Count)
				{
					throw new Exception($"Scene portal count {scene.SizePortals} doesn't match with given {cullingSetting.Portals_C29.Count}");
				}
				SetIDs(occlusionCullingData, container, cullingSetting, scene);
			}
		}

		private static void SetIDs(this IOcclusionCullingData occlusionCullingData, IExportContainer container, IOcclusionCullingSettings cullingSetting, IOcclusionScene scene)
		{
			PPtrAccessList<PPtr_Renderer__5_0_0_f4, IRenderer> renderers = cullingSetting.StaticRenderers_C29P;
			for (int i = 0; i < renderers.Count; i++)
			{
				IRenderer? renderer = renderers[i];
				occlusionCullingData.StaticRenderers_C363[scene.IndexRenderers + i].SetObjectID(container, renderer);
			}

			PPtrAccessList<PPtr_OcclusionPortal__5_5_0_f3, IOcclusionPortal> portals = cullingSetting.Portals_C29P;
			for (int i = 0; i < portals.Count; i++)
			{
				IOcclusionPortal? portal = portals[i];
				occlusionCullingData.Portals_C363[scene.IndexPortals + i].SetObjectID(container, portal);
			}
		}

		private static void SetObjectID(this ISceneObjectIdentifier sceneObjectIdentifier, IExportContainer container, IUnityObjectBase? asset)
		{
			if (sceneObjectIdentifier is null)
			{
				throw new ArgumentNullException(nameof(sceneObjectIdentifier));
			}

			long lid = asset == null ? 0 : container.GetExportID(asset);
			sceneObjectIdentifier.TargetObject = lid;
			sceneObjectIdentifier.TargetPrefab = 0;
		}
	}
}
