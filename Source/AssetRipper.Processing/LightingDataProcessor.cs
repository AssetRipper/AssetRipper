using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;
using AssetRipper.SourceGenerated.Classes.ClassID_108;
using AssetRipper.SourceGenerated.Classes.ClassID_1120;
using AssetRipper.SourceGenerated.Classes.ClassID_157;
using AssetRipper.SourceGenerated.Classes.ClassID_218;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_258;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.LightmapData;
using AssetRipper.SourceGenerated.Subclasses.RendererData;
using AssetRipper.SourceGenerated.Subclasses.SceneObjectIdentifier;

namespace AssetRipper.Processing
{
	public class LightingDataProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Lighting Data Assets");
			ProcessedAssetCollection processedCollection = gameBundle.AddNewProcessedCollection("Generated Lighting Data Assets", projectVersion);

			foreach (AssetCollection collection in gameBundle.FetchAssetCollections())
			{
				if (!collection.IsScene)
				{
					//Only scenes can contain a LightmapSettings asset.
					continue;
				}

				ILightmapSettings? lightmapSettings = collection.OfType<ILightmapSettings>().FirstOrDefault();
				if (lightmapSettings is not null && (lightmapSettings.Has_LightingDataAsset_C157() || lightmapSettings.Has_LightmapSnapshot_C157()))
				{
					ILightingDataAsset lightingDataAsset = CreateLightingDataAsset(processedCollection);

					PPtrConverter converter = new PPtrConverter(lightmapSettings, lightingDataAsset);

					foreach (ILightmapData lightmapData in lightmapSettings.Lightmaps_C157)
					{
						lightingDataAsset.Lightmaps_C1120.AddNew().CopyValues(lightmapData, converter);
					}

					lightingDataAsset.LightmapsMode_C1120 = lightmapSettings.LightmapsMode_C157;

					lightingDataAsset.EnlightenSceneMapping_C1120.CopyValues(lightmapSettings.EnlightenSceneMapping_C157, converter);

					if (lightingDataAsset.Has_Scene_C1120())
					{
						ISceneAsset sceneAsset = CreateSceneAsset(processedCollection, collection);
						lightingDataAsset.Scene_C1120P = sceneAsset;
					}
					else if (lightingDataAsset.Has_SceneGUID_C1120())
					{
						lightingDataAsset.SceneGUID_C1120.CopyValues(collection.Scene.GUID);
					}

					lightingDataAsset.LightProbes_C1120P = (ILightProbes?)lightmapSettings.LightProbes_C157P;
					//Note: it is possible for a LightProbes asset to be shared between multiple LightingDataAsset.
					//However, that happened when multiple scenes were loaded additively and baked together.
					//In that situation, the LightProbes asset and each LightingDataAsset were all in one binary file.
					//A LightingDataAssetParent was also in the file and acted as the main asset in the NativeFormatImporter.

					lightingDataAsset.EnlightenDataVersion_C1120 = 112;
					//This must be assigned correctly. The version varies widely based on Unity version.
					//It seems that -1 will not suffice. 112 is the version that 2021.1 and 2021.2 use.
					//Since Enlighten is no longer being maintained, any later version should also use 112.
					//Supposedly, 112 has been in use since 2017 or possibly even late Unity 5.
					//To extract the Enlighten version for each Unity version, one would have to create
					//a test project on each version and then bake the lighting in the test project.
					//There is no proper API to create a LightingDataAsset.

					lightingDataAsset.NameString = collection.Scene.Name;
					//Normally, the asset is called "LightingData" but we give it a more unique name here
					//because any others will be in the same folder.

					if (lightmapSettings.Has_LightingDataAsset_C157())
					{
						lightmapSettings.LightingDataAsset_C157P = lightingDataAsset;
					}
					else if (lightmapSettings.Has_LightmapSnapshot_C157())
					{
						lightmapSettings.LightmapSnapshot_C157P = lightingDataAsset;
					}

					foreach (IUnityObjectBase asset in collection)
					{
						if (asset is IRenderer renderer) //Need to do all renderer types
						{
							ushort lightmapIndex = renderer.GetLightmapIndex();
							if (lightmapIndex == ushort.MaxValue && renderer.LightmapIndexDynamic_C25 == ushort.MaxValue)
							{
								// No lightmap data associated with renderer
								continue;
							}

							//Scene object identifiers for the renderer associated with each value in the lightmapped renderer data array
							SceneObjectIdentifier identifier = lightingDataAsset.LightmappedRendererDataIDs_C1120.AddNew();
							identifier.TargetObjectReference = asset;

							//The lightmap index, lightmap uv scale/offset value, etc
							IRendererData rendererData = lightingDataAsset.LightmappedRendererData_C1120.AddNew();
							rendererData.LightmapIndex = lightmapIndex;
							rendererData.LightmapIndexDynamic = renderer.LightmapIndexDynamic_C25;
							rendererData.LightmapST.CopyValues(renderer.LightmapTilingOffset_C25);
							if (renderer.Has_LightmapTilingOffsetDynamic_C25())
							{
								rendererData.LightmapSTDynamic.CopyValues(renderer.LightmapTilingOffsetDynamic_C25);
							}
						}
						else if (asset is ITerrain terrain)
						{
							if (terrain.LightmapIndex_C218 == ushort.MaxValue && terrain.LightmapIndexDynamic_C218 == ushort.MaxValue)
							{
								// No lightmap data associated with the terrain
								continue;
							}

							//Scene object identifiers for the terrain associated with each value in the lightmapped renderer data array
							SceneObjectIdentifier identifier = lightingDataAsset.LightmappedRendererDataIDs_C1120.AddNew();
							identifier.TargetObjectReference = asset;

							//The lightmap index, lightmap uv scale/offset value, etc
							IRendererData rendererData = lightingDataAsset.LightmappedRendererData_C1120.AddNew();
							rendererData.LightmapIndex = terrain.LightmapIndex_C218;
							rendererData.LightmapIndexDynamic = terrain.LightmapIndexDynamic_C218;
							rendererData.LightmapST.CopyValues(terrain.LightmapTilingOffset_C218);
							rendererData.LightmapSTDynamic.CopyValues(terrain.LightmapTilingOffsetDynamic_C218);

							rendererData.TerrainDynamicUVST.CopyValues(terrain.DynamicUVST_C218);
							rendererData.TerrainChunkDynamicUVST.CopyValues(terrain.ChunkDynamicUVST_C218);
							rendererData.ExplicitProbeSetHash?.CopyValues(terrain.ExplicitProbeSetHash_C218);
						}
						else if (asset is ILight light)
						{
							// We're not sure what the most appropriate way to check if a light belongs
							// in these arrays or not is, but just including all of them is harmless.

							SceneObjectIdentifier identifier = lightingDataAsset.Lights_C1120.AddNew();
							identifier.TargetObjectReference = asset;

							lightingDataAsset.LightBakingOutputs_C1120?.AddNew().CopyValues(light.BakingOutput_C108);
						}
					}

					//Todo:
					//As described in AssetRipper#553, LightingDataAsset, LightProbes, and the lightmap textures
					//should all be exported in a subfolder beside the scene. This folder has the same name as the scene.
				}
			}
		}

		private static ILightingDataAsset CreateLightingDataAsset(ProcessedAssetCollection collection)
		{
			return collection.CreateAsset((int)ClassIDType.LightingDataAsset, LightingDataAssetFactory.CreateAsset);
		}

		private static ISceneAsset CreateSceneAsset(ProcessedAssetCollection collection, AssetCollection targetScene)
		{
			ISceneAsset asset = collection.CreateAsset((int)ClassIDType.SceneAsset, SceneAssetFactory.CreateAsset);
			asset.TargetScene = targetScene;
			return asset;
		}
	}
}
