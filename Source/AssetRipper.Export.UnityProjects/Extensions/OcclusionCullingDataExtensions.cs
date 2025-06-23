using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_29;
using AssetRipper.SourceGenerated.Classes.ClassID_363;
using AssetRipper.SourceGenerated.Classes.ClassID_41;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.GUID;
using AssetRipper.SourceGenerated.Subclasses.OcclusionScene;
using AssetRipper.SourceGenerated.Subclasses.PPtr_OcclusionPortal;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Renderer;
using AssetRipper.SourceGenerated.Subclasses.SceneObjectIdentifier;

namespace AssetRipper.Export.UnityProjects.Extensions;

internal static class OcclusionCullingDataExtensions
{
	public static void Initialize(this IOcclusionCullingData occlusionCullingData, IExportContainer container, IOcclusionCullingSettings cullingSetting)
	{
		occlusionCullingData.PVSData = cullingSetting.PVSData ?? Array.Empty<byte>();
		int renderCount = cullingSetting.StaticRenderers?.Count ?? 0;
		int portalCount = cullingSetting.Portals?.Count ?? 0;
		occlusionCullingData.Scenes.InitializeList(1);
		OcclusionScene scene = occlusionCullingData.Scenes[0];
		scene.Initialize(cullingSetting.SceneGUID, renderCount, portalCount);

		occlusionCullingData.StaticRenderers.InitializeList(scene.SizeRenderers);
		occlusionCullingData.Portals.InitializeList(scene.SizePortals);
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
		// If < 3.0.0, this asset doesn't exist.

		// For 3.0.0 to 5.5.0, this asset is created by culling settings, so its data is already set.
		if (occlusionCullingData.Collection.Version.LessThan(5, 5))
		{
			return;
		}

		// If >= 5.5.0 and not Release, this asset contains renderer data,
		if (!occlusionCullingData.Collection.Flags.IsRelease())
		{
			return;
		}

		// If >= 5.5.0 and Release. this asset doesn't contain renderer data, so we need to create it.
		List<IOcclusionCullingSettings> cullingSettings = new();
		foreach (IUnityObjectBase asset in occlusionCullingData.Collection.Bundle.FetchAssetsInHierarchy())
		{
			if (asset is IOcclusionCullingSettings cullingSetting
				&& cullingSetting.Has_OcclusionCullingData()
				&& cullingSetting.OcclusionCullingData.IsAsset(cullingSetting.Collection, occlusionCullingData))
			{
				cullingSettings.Add(cullingSetting);
			}
		}

		int maxRenderer = occlusionCullingData.Scenes.Max(j => j.IndexRenderers + j.SizeRenderers);
		occlusionCullingData.StaticRenderers.InitializeList(maxRenderer);
		int maxPortal = occlusionCullingData.Scenes.Max(j => j.IndexPortals + j.SizePortals);
		occlusionCullingData.Portals.InitializeList(maxPortal);

		foreach (IOcclusionCullingSettings cullingSetting in cullingSettings)
		{
			if (!cullingSetting.Has_SceneGUID())
			{
				continue;
			}

			int sceneIndex = occlusionCullingData.Scenes.IndexOf(t => t.Scene == cullingSetting.SceneGUID);
			if (sceneIndex == -1)
			{
				//Previously a logged error
				throw new Exception($"Unable to find scene data with GUID {cullingSetting.SceneGUID} in {occlusionCullingData.GetBestName()}");
			}

			IOcclusionScene scene = occlusionCullingData.Scenes[sceneIndex];
			if (scene.SizeRenderers != cullingSetting.StaticRenderers.Count)
			{
				throw new Exception($"Scene renderer count {scene.SizeRenderers} doesn't match with given {cullingSetting.StaticRenderers.Count}");
			}
			if (scene.SizePortals != cullingSetting.Portals.Count)
			{
				throw new Exception($"Scene portal count {scene.SizePortals} doesn't match with given {cullingSetting.Portals.Count}");
			}
			occlusionCullingData.SetIDs(container, cullingSetting, scene);
		}
	}

	private static void SetIDs(this IOcclusionCullingData occlusionCullingData, IExportContainer container, IOcclusionCullingSettings cullingSetting, IOcclusionScene scene)
	{
		PPtrAccessList<PPtr_Renderer_5, IRenderer> renderers = cullingSetting.StaticRenderersP;
		for (int i = 0; i < renderers.Count; i++)
		{
			IRenderer? renderer = renderers[i];
			occlusionCullingData.StaticRenderers[scene.IndexRenderers + i].SetObjectID(container, renderer);
		}

		PPtrAccessList<PPtr_OcclusionPortal_5_5, IOcclusionPortal> portals = cullingSetting.PortalsP;
		for (int i = 0; i < portals.Count; i++)
		{
			IOcclusionPortal? portal = portals[i];
			occlusionCullingData.Portals[scene.IndexPortals + i].SetObjectID(container, portal);
		}
	}

	private static void SetObjectID(this ISceneObjectIdentifier sceneObjectIdentifier, IExportContainer container, IUnityObjectBase? asset)
	{
		ArgumentNullException.ThrowIfNull(sceneObjectIdentifier);

		long lid = asset == null ? 0 : container.GetExportID(asset);
		sceneObjectIdentifier.TargetObject = lid;
		sceneObjectIdentifier.TargetPrefab = 0;
	}
}
