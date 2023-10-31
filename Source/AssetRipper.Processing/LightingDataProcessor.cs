﻿using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;
using AssetRipper.SourceGenerated.Classes.ClassID_108;
using AssetRipper.SourceGenerated.Classes.ClassID_1120;
using AssetRipper.SourceGenerated.Classes.ClassID_157;
using AssetRipper.SourceGenerated.Classes.ClassID_218;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_258;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.LightmapData;
using AssetRipper.SourceGenerated.Subclasses.RendererData;
using AssetRipper.SourceGenerated.Subclasses.SceneObjectIdentifier;

namespace AssetRipper.Processing
{
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

			foreach (SceneDefinition scene in gameData.GameBundle.Scenes)
			{
				//Only scenes can contain a LightmapSettings asset.
				ILightmapSettings? lightmapSettings = scene.Assets.OfType<ILightmapSettings>().FirstOrDefault();
				if (lightmapSettings is null)
				{
					continue;
				}

				lightmapSettingsDictionary.Add(lightmapSettings, scene);
				if (lightmapSettings.LightProbesP is { } lightProbes && !lightProbeDictionary.TryAdd(lightProbes, scene))
				{
					lightProbeDictionary[lightProbes] = null;//This set of light probes is shared between scenes.
				}

				if (!lightmapSettings.Has_LightingDataAsset() || !HasLightingData(lightmapSettings))
				{
					continue;
				}

				ILightingDataAsset lightingDataAsset = CreateLightingDataAsset(processedCollection);

				lightmapSettings.LightingDataAssetP = lightingDataAsset;

				PPtrConverter converter = new PPtrConverter(lightmapSettings, lightingDataAsset);

				lightingDataAsset.LightmapsMode = lightmapSettings.LightmapsMode;
				lightingDataAsset.EnlightenSceneMapping.CopyValues(lightmapSettings.EnlightenSceneMapping, converter);

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
				SetPathsAndMainAsset(lightmapSettings, lightProbes, scene);
			}
		}

		private static bool HasLightingData(ILightmapSettings lightmapSettings)
		{
			//Supposedly, a LightingDataAsset without any lightmaps causes Unity to crash.
			//See: https://github.com/AssetRipper/AssetRipper/issues/811
			return lightmapSettings.Lightmaps.Count > 0;
		}

		private static void AddRenderer(ILightingDataAsset lightingDataAsset, IRenderer renderer)
		{
			ushort lightmapIndex = renderer.GetLightmapIndex();
			if (lightmapIndex != ushort.MaxValue || renderer.LightmapIndexDynamic_C25 != ushort.MaxValue)
			{
				//Scene object identifiers for the renderer associated with each value in the lightmapped renderer data array
				SceneObjectIdentifier identifier = lightingDataAsset.LightmappedRendererDataIDs.AddNew();
				identifier.TargetObjectReference = renderer;

				//The lightmap index, lightmap uv scale/offset value, etc
				IRendererData rendererData = lightingDataAsset.LightmappedRendererData.AddNew();
				rendererData.LightmapIndex = lightmapIndex;
				rendererData.LightmapIndexDynamic = renderer.LightmapIndexDynamic_C25;
				rendererData.LightmapST.CopyValues(renderer.LightmapTilingOffset_C25);
				rendererData.LightmapSTDynamic.CopyValues(renderer.LightmapTilingOffsetDynamic_C25);
			}
			else
			{
				// No lightmap data associated with the renderer
			}
		}

		private static void AddTerrain(ILightingDataAsset lightingDataAsset, ITerrain terrain)
		{
			if (terrain.LightmapIndex != ushort.MaxValue || terrain.LightmapIndexDynamic != ushort.MaxValue)
			{
				//Scene object identifiers for the terrain associated with each value in the lightmapped renderer data array
				SceneObjectIdentifier identifier = lightingDataAsset.LightmappedRendererDataIDs.AddNew();
				identifier.TargetObjectReference = terrain;

				//The lightmap index, lightmap uv scale/offset value, etc
				IRendererData rendererData = lightingDataAsset.LightmappedRendererData.AddNew();
				rendererData.LightmapIndex = terrain.LightmapIndex;
				rendererData.LightmapIndexDynamic = terrain.LightmapIndexDynamic;
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

			lightingDataAsset.LightBakingOutputs?.AddNew().CopyValues(light.BakingOutput);
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

		private static void SetPathsAndMainAsset(ILightmapSettings lightmapSettings, ILightProbes? lightProbes, SceneDefinition scene)
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
			lightingDataAsset.LightProbesP = lightmapSettings.LightProbesP as ILightProbes;
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

		/// <summary>
		/// Create a new, empty <see cref="ILightingDataAsset"/>.
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		private static ILightingDataAsset CreateLightingDataAsset(ProcessedAssetCollection collection)
		{
			return collection.CreateAsset((int)ClassIDType.LightingDataAsset, LightingDataAsset.Create);
		}

		private static ISceneAsset CreateSceneAsset(ProcessedAssetCollection collection, SceneDefinition targetScene)
		{
			ISceneAsset asset = collection.CreateAsset((int)ClassIDType.SceneAsset, SceneAsset.Create);
			asset.TargetScene = targetScene;
			return asset;
		}
	}
}
