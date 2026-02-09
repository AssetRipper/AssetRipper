using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.SourceGenerated.Classes.ClassID_0;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;
using AssetRipper.SourceGenerated.Classes.ClassID_104;
using AssetRipper.SourceGenerated.Classes.ClassID_108;
using AssetRipper.SourceGenerated.Classes.ClassID_1120;
using AssetRipper.SourceGenerated.Classes.ClassID_157;
using AssetRipper.SourceGenerated.Classes.ClassID_218;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_258;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_850595691;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.LightmapData;
using AssetRipper.SourceGenerated.Subclasses.RendererData;
using AssetRipper.SourceGenerated.Subclasses.SceneObjectIdentifier;

namespace AssetRipper.Processing;

public class LightingDataProcessor : IAssetProcessor
{
	/// <summary>
	/// The default name of newly created <see cref="ILightingDataAsset"/>s.
	/// </summary>
	private static Utf8String LightingDataName { get; } = new("LightingData");

	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Lighting Data Assets");
		ProcessedAssetCollection processedCollection = gameData.AddNewProcessedCollection("Generated Lighting Data Assets");

		Dictionary<ILightmapSettings, SceneDefinition> lightmapSettingsDictionary = new();
		Dictionary<ILightProbes, SceneDefinition?> lightProbeDictionary = new();
		Dictionary<ILightingSettings, SceneDefinition?> lightingSettingsDictionary = new();

		foreach (SceneDefinition scene in gameData.GameBundle.Scenes)
		{
			//Only scenes can contain a LightmapSettings asset.
			ILightmapSettings? lightmapSettings = scene.Assets.OfType<ILightmapSettings>().FirstOrDefault();
			if (lightmapSettings is null)
			{
				continue;
			}

			IRenderSettings? renderSettings = scene.Assets.OfType<IRenderSettings>().FirstOrDefault();
			if (renderSettings is null)
			{
				// This should never happen. All scenes need a RenderSettings asset.
				continue;
			}

			lightmapSettingsDictionary.Add(lightmapSettings, scene);

			if (lightmapSettings.LightProbesP is { } lightProbes && !lightProbeDictionary.TryAdd(lightProbes, scene))
			{
				//This set of light probes is shared between scenes.
				lightProbeDictionary[lightProbes] = null;
			}

			if (lightmapSettings.LightingSettingsP is { } lightingSettings && !lightingSettingsDictionary.TryAdd(lightingSettings, scene))
			{
				//This LightingSettings is shared between scenes.
				lightingSettingsDictionary[lightingSettings] = null;
			}

			if (!lightmapSettings.Has_LightingDataAsset())
			{
				continue;
			}

			ILightingDataAsset lightingDataAsset = processedCollection.CreateLightingDataAsset();

			lightmapSettings.LightingDataAssetP = lightingDataAsset;

			PPtrConverter converter = new PPtrConverter(lightmapSettings, lightingDataAsset);

			lightingDataAsset.LightmapsMode = lightmapSettings.LightmapsMode;
			lightingDataAsset.EnlightenData = CreateEnlightenData(lightingDataAsset.Collection.Version);

			SetEnlightenSceneMapping(lightingDataAsset, lightmapSettings, converter);
			SetBakedAmbientProbes(lightingDataAsset, renderSettings);
			AddSkyboxReflection(lightingDataAsset, renderSettings);
			SetLightmaps(lightingDataAsset, lightmapSettings.Lightmaps, converter);
			SetScene(lightingDataAsset, scene, processedCollection);
			SetLightProbes(lightingDataAsset, lightmapSettings);
			SetEnlightenDataVersion(lightingDataAsset);

			foreach (IUnityObjectBase asset in scene.Assets)
			{
				switch (asset)
				{
					case IRenderer renderer:
						AddRenderer(lightingDataAsset, renderer);
						break;
					case ITerrain terrain:
						AddTerrain(lightingDataAsset, terrain);
						break;
					case ILight light:
						AddLight(lightingDataAsset, light);
						break;
				}
			}
		}

		foreach ((ILightmapSettings lightmapSettings, SceneDefinition scene) in lightmapSettingsDictionary)
		{
			ILightProbes? lightProbes = lightmapSettings.LightProbesP;
			if (lightProbes is not null && lightProbeDictionary[lightProbes] is null)
			{
				lightProbes = null;//Shared light probes should not have their path set.
			}

			ILightingSettings? lightingSettings = lightmapSettings.LightingSettingsP;
			if (lightingSettings is not null && lightingSettingsDictionary[lightingSettings] is null)
			{
				lightingSettings = null;//Shared light settings should not have their path set.
			}

			SetPathsAndMainAsset(lightmapSettings, lightProbes, lightingSettings, scene);
		}
	}

	private static void AddRenderer(ILightingDataAsset lightingDataAsset, IRenderer renderer)
	{
		//-1 indicates that it's not part of the lightmap.
		ushort lightmapIndex = renderer.GetLightmapIndex();
		if (lightmapIndex != ushort.MaxValue)// || renderer.LightmapIndexDynamic_C25 != ushort.MaxValue)
		{
			//Scene object identifiers for the renderer associated with each value in the lightmapped renderer data array
			SceneObjectIdentifier identifier = lightingDataAsset.LightmappedRendererDataIDs.AddNew();
			identifier.TargetObjectReference = renderer;

			//The lightmap index, lightmap uv scale/offset value, etc
			IRendererData rendererData = lightingDataAsset.LightmappedRendererData.AddNew();
			rendererData.LightmapIndex = lightmapIndex;

			//This seems to crash the editor when it's not set to -1.
			//See: https://github.com/AssetRipper/AssetRipper/issues/811
			rendererData.LightmapIndexDynamic = ushort.MaxValue;//renderer.LightmapIndexDynamic_C25;

			rendererData.LightmapST.CopyValues(renderer.LightmapTilingOffset_C25);
			rendererData.LightmapSTDynamic.CopyValues(renderer.LightmapTilingOffsetDynamic_C25);
			rendererData.UvMesh.SetAsset(lightingDataAsset.Collection, renderer.GameObject_C25P?.TryGetComponent<IMeshFilter>()?.MeshP);
		}
		else
		{
			// No lightmap data associated with the renderer
		}
	}

	private static void AddTerrain(ILightingDataAsset lightingDataAsset, ITerrain terrain)
	{
		//-1 indicates that it's not part of the lightmap.
		if (terrain.LightmapIndex != ushort.MaxValue)// || terrain.LightmapIndexDynamic != ushort.MaxValue)
		{
			//Scene object identifiers for the terrain associated with each value in the lightmapped renderer data array
			SceneObjectIdentifier identifier = lightingDataAsset.LightmappedRendererDataIDs.AddNew();
			identifier.TargetObjectReference = terrain;

			//The lightmap index, lightmap uv scale/offset value, etc
			IRendererData rendererData = lightingDataAsset.LightmappedRendererData.AddNew();
			rendererData.LightmapIndex = terrain.LightmapIndex;

			//This seems to crash the editor when it's not set to -1.
			//See: https://github.com/AssetRipper/AssetRipper/issues/811
			rendererData.LightmapIndexDynamic = ushort.MaxValue;//terrain.LightmapIndexDynamic;

			rendererData.LightmapST.CopyValues(terrain.LightmapTilingOffset);
			rendererData.LightmapSTDynamic.CopyValues(terrain.LightmapTilingOffsetDynamic);

			rendererData.TerrainDynamicUVST.CopyValues(terrain.DynamicUVST);
			rendererData.TerrainChunkDynamicUVST.CopyValues(terrain.ChunkDynamicUVST);
			rendererData.ExplicitProbeSetHash?.CopyValues(terrain.ExplicitProbeSetHash);
		}
		else
		{
			// No lightmap data associated with the terrain
		}
	}

	private static void AddLight(ILightingDataAsset lightingDataAsset, ILight light)
	{
		// We're not sure what the most appropriate way to check if a light belongs
		// in these arrays or not is, but just including all of them is harmless.

		SceneObjectIdentifier identifier = lightingDataAsset.Lights.AddNew();
		identifier.TargetObjectReference = light;

		//Information about whether a light is baked or not
		if (light.Has_BakingOutput())
		{
			lightingDataAsset.LightBakingOutputs?.AddNew().CopyValues(light.BakingOutput);
		}
	}

	private static void SetEnlightenSceneMapping(ILightingDataAsset lightingDataAsset, ILightmapSettings lightmapSettings, PPtrConverter converter)
	{
		lightingDataAsset.EnlightenSceneMapping.CopyValues(lightmapSettings.EnlightenSceneMapping, converter);

		foreach (IObject? renderer in lightingDataAsset.EnlightenSceneMapping.Renderers.Select(r => r.Renderer.TryGetAsset(lightingDataAsset.Collection)))
		{
			lightingDataAsset.EnlightenSceneMappingRendererIDs.AddNew().TargetObjectReference = renderer;
		}
	}

	private static void SetBakedAmbientProbes(ILightingDataAsset lightingDataAsset, IRenderSettings renderSettings)
	{
		if (renderSettings.Has_AmbientProbeInGamma())
		{
			if (lightingDataAsset.Has_BakedAmbientProbeInGamma())
			{
				lightingDataAsset.BakedAmbientProbeInGamma.CopyValues(renderSettings.AmbientProbeInGamma);
			}
			else if (lightingDataAsset.Has_BakedAmbientProbesInGamma())
			{
				lightingDataAsset.BakedAmbientProbesInGamma.AddNew().CopyValues(renderSettings.AmbientProbeInGamma);
			}
		}
		if (renderSettings.Has_AmbientProbe())
		{
			if (lightingDataAsset.Has_BakedAmbientProbeInLinear())
			{
				lightingDataAsset.BakedAmbientProbeInLinear.CopyValues(renderSettings.AmbientProbe);
			}
			else if (lightingDataAsset.Has_BakedAmbientProbesInLinear())
			{
				lightingDataAsset.BakedAmbientProbesInLinear.AddNew().CopyValues(renderSettings.AmbientProbe);
			}
		}
	}

	private static void AddSkyboxReflection(ILightingDataAsset lightingDataAsset, IRenderSettings renderSettings)
	{
		if (renderSettings.GeneratedSkyboxReflectionP is { } skyboxReflection)
		{
			lightingDataAsset.BakedReflectionProbeCubemapsP.Add(skyboxReflection);
		}
	}

	/// <summary>
	/// Add several <see cref="ILightmapData"/> to <see cref="ILightingDataAsset.Lightmaps"/>.
	/// </summary>
	/// <param name="lightingDataAsset"></param>
	/// <param name="lightmaps"></param>
	/// <param name="converter"></param>
	private static void SetLightmaps(ILightingDataAsset lightingDataAsset, AccessListBase<ILightmapData> lightmaps, PPtrConverter converter)
	{
		foreach (ILightmapData lightmapData in lightmaps)
		{
			lightingDataAsset.Lightmaps.AddNew().CopyValues(lightmapData, converter);
		}
	}

	private static void SetPathsAndMainAsset(ILightmapSettings lightmapSettings, ILightProbes? lightProbes, ILightingSettings? lightingSettings, SceneDefinition scene)
	{
		//Several assets should all be exported in a subfolder beside the scene.
		//Example:
		//Scenes
		//  MyScene.unity
		//  MyScene //This folder has the same name as the scene.
		//    LightingData.asset //This is the default name from Unity.
		//    LightProbes.asset //optional; this can be anywhere
		//    <a bunch of lightmap textures> //optional; the textures can be anywhere

		ILightingDataAsset? lightingDataAsset = lightmapSettings.LightingDataAssetP;
		if (lightingDataAsset is not null)
		{
			lightingDataAsset.MainAsset = lightingDataAsset;

			lightingDataAsset.OriginalDirectory ??= scene.Path;
			if (lightingDataAsset.Name.IsEmpty)
			{
				lightingDataAsset.Name = LightingDataName;
			}

			//This OriginalName is purely for the UI. Name is used for exporting the asset.
			lightingDataAsset.OriginalName ??= scene.Name;
		}

		//Move the light probes to the scene subfolder if it exists and is not shared with other scenes.
		if (lightProbes is not null)
		{
			lightProbes.OriginalDirectory ??= scene.Path;
		}

		//Move the light settings to the scene subfolder if it exists and is not shared with other scenes.
		//There's no requirement to place it there, but it helps with organization.
		//This is particularly useful when many LightingSettings have the same name.
		if (lightingSettings is not null)
		{
			lightingSettings.OriginalDirectory ??= scene.Path;
		}

		//Move the lightmap textures to the scene subfolder.
		foreach (ILightmapData lightmapData in lightmapSettings.Lightmaps)
		{
			if (lightmapData.DirLightmap?.TryGetAsset(lightmapSettings.Collection, out ITexture2D? dirLightmap) ?? false)
			{
				dirLightmap.OriginalDirectory ??= scene.Path;
				dirLightmap.MainAsset = lightingDataAsset;
			}
			if (lightmapData.IndirectLightmap?.TryGetAsset(lightmapSettings.Collection, out ITexture2D? indirectLightmap) ?? false)
			{
				indirectLightmap.OriginalDirectory ??= scene.Path;
				indirectLightmap.MainAsset = lightingDataAsset;
			}
			if (lightmapData.Lightmap.TryGetAsset(lightmapSettings.Collection, out ITexture2D? lightmap))
			{
				lightmap.OriginalDirectory ??= scene.Path;
				lightmap.MainAsset = lightingDataAsset;
			}
			if (lightmapData.ShadowMask?.TryGetAsset(lightmapSettings.Collection, out ITexture2D? shadowMask) ?? false)
			{
				shadowMask.OriginalDirectory ??= scene.Path;
				shadowMask.MainAsset = lightingDataAsset;
			}
		}
	}

	/// <summary>
	/// Sets <see cref="ILightingDataAsset.LightProbesP"/> from <see cref="ILightmapSettings.LightProbesP"/>.
	/// </summary>
	/// <remarks>
	/// Note: it is possible for a LightProbes asset to be shared between multiple LightingDataAsset.<br/>
	/// However, that happened when multiple scenes were loaded additively and baked together.<br/>
	/// In that situation, the LightProbes asset and each LightingDataAsset were all in one binary file.<br/>
	/// A LightingDataAssetParent was also in the file and acted as the main asset in the NativeFormatImporter.
	/// </remarks>
	/// <param name="lightingDataAsset"></param>
	/// <param name="lightmapSettings"></param>
	private static void SetLightProbes(ILightingDataAsset lightingDataAsset, ILightmapSettings lightmapSettings)
	{
		lightingDataAsset.LightProbesP = lightmapSettings.LightProbesP;
	}

	/// <summary>
	/// Sets <see cref="ILightingDataAsset.SceneP"/> or <see cref="ILightingDataAsset.SceneGUID"/>.
	/// </summary>
	/// <param name="lightingDataAsset"></param>
	/// <param name="scene"></param>
	/// <param name="processedCollection"></param>
	private static void SetScene(ILightingDataAsset lightingDataAsset, SceneDefinition scene, ProcessedAssetCollection processedCollection)
	{
		if (lightingDataAsset.Has_Scene())
		{
			ISceneAsset sceneAsset = CreateSceneAsset(processedCollection, scene);
			lightingDataAsset.SceneP = sceneAsset;
		}
		else if (lightingDataAsset.Has_SceneGUID())
		{
			lightingDataAsset.SceneGUID.CopyValues(scene.GUID);
		}
	}

	/// <summary>
	/// Sets <see cref="ILightingDataAsset.EnlightenDataVersion"/>
	/// </summary>
	/// <remarks>
	/// This value must be assigned correctly. The version varies widely based on Unity version.<br/>
	/// It seems that -1 will not suffice. 112 is the version that 2021.1 and 2021.2 use.<br/>
	/// Since Enlighten is no longer being maintained, any later version should also use 112.<br/>
	/// Supposedly, 112 has been in use since 2017 or possibly even late Unity 5.<br/>
	/// To extract the Enlighten version for each Unity version, one would have to create
	/// a test project on each version and then bake the lighting in the test project.<br/>
	/// There is no proper API to create a LightingDataAsset.
	/// </remarks>
	/// <param name="lightingDataAsset"></param>
	private static void SetEnlightenDataVersion(ILightingDataAsset lightingDataAsset)
	{
		lightingDataAsset.EnlightenDataVersion = 112;
	}

	private static ISceneAsset CreateSceneAsset(ProcessedAssetCollection collection, SceneDefinition targetScene)
	{
		ISceneAsset asset = collection.CreateSceneAsset();
		asset.TargetScene = targetScene;
		return asset;
	}

	private static byte[] CreateEnlightenData(UnityVersion version)
	{
		// For many of the lighting data assets I've encountered, they just contained the bytes of an empty asset bundle.
		BundleVersion bundleVersion = false switch
		{
			_ when version.GreaterThanOrEquals(2022, 2) => BundleVersion.BF_2022_2,
			_ when version.GreaterThanOrEquals(2020) => BundleVersion.BF_LargeFilesSupport, // This started sometime during 2019.4.X, so we use 2020 just to be safe.
			_ when version.GreaterThanOrEquals(5, 2, 0, UnityVersionType.Final) => BundleVersion.BF_520_x,
			_ => BundleVersion.BF_350_4x,
		};

		FileStreamBundleFile bundle = new();
		FileStreamBundleHeader header = bundle.Header;
		header.Version = bundleVersion;
		header.UnityWebBundleVersion = "5.x.x";
		header.UnityWebMinimumRevision = version.ToString();

		using MemoryStream stream = new();
		bundle.Write(stream);

		return stream.ToArray();
	}
}
