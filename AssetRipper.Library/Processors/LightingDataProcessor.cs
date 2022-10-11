using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Linq;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1120;
using AssetRipper.SourceGenerated.Classes.ClassID_157;
using AssetRipper.SourceGenerated.Subclasses.EnlightenRendererInformation;
using AssetRipper.SourceGenerated.Subclasses.EnlightenSystemAtlasInformation;
using AssetRipper.SourceGenerated.Subclasses.EnlightenSystemInformation;
using AssetRipper.SourceGenerated.Subclasses.EnlightenTerrainChunksInformation;
using AssetRipper.SourceGenerated.Subclasses.Hash128;
using AssetRipper.SourceGenerated.Subclasses.LightmapData;
using System.Linq;

namespace AssetRipper.Library.Processors
{
	public class LightingDataProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Lighting Data Assets");
			ProcessedAssetCollection processedCollection = CreateProcessedCollection(gameBundle, projectVersion);

			foreach (AssetCollection collection in gameBundle.FetchAssetCollections())
			{
				ILightmapSettings? lightmapSettings = collection.SelectType<IUnityObjectBase, ILightmapSettings>().FirstOrDefault();
				if (lightmapSettings is not null && (lightmapSettings.Has_LightingDataAsset_C157() || lightmapSettings.Has_LightmapSnapshot_C157()))
				{
					ILightingDataAsset lightingDataAsset = CreateLightingDataAsset(processedCollection);

					foreach (ILightmapData lightmapData in lightmapSettings.Lightmaps_C157)
					{
						ILightmapData newLightmapData = lightingDataAsset.Lightmaps_C1120.AddNew();
						SetPPtr(newLightmapData.DirLightmap, processedCollection, lightmapData.DirLightmap, collection);
						SetPPtr(newLightmapData.IndirectLightmap, processedCollection, lightmapData.IndirectLightmap, collection);
						SetPPtr(newLightmapData.Lightmap, processedCollection, lightmapData.Lightmap, collection);
						SetPPtr(newLightmapData.ShadowMask, processedCollection, lightmapData.ShadowMask, collection);
					}

					lightingDataAsset.LightmapsMode_C1120 = lightmapSettings.LightmapsMode_C157;

					CopyEnlightenSceneMapping(lightingDataAsset, lightmapSettings);

					SetPPtr(lightingDataAsset.LightProbes_C1120, processedCollection, lightmapSettings.LightProbes_C157, collection);

					lightingDataAsset.EnlightenDataVersion_C1120 = 112;
					//This must be assigned correctly. The version varies widely based on Unity version.
					//It seems that -1 will not suffice. 112 is the version that 2021.1 and 2021.2 use.
					//Since Enlighten is no longer being maintained, any later version should also use 112.
					//Supposedly, 112 has been in use since 2017 or possibly even late Unity 5.
					//To extract the Enlighten version for each Unity version, one would have to create
					//a test project on each version and then bake the lighting in the test project.
					//There is no proper API to create a LightingDataAsset.

					lightingDataAsset.NameString = collection.Name;
					//Normally, the asset is called "LightingData" but we give it a more unique name here
					//because any others will be in the same folder.

					if (lightmapSettings.Has_LightingDataAsset_C157())
					{
						lightmapSettings.LightingDataAsset_C157.CopyValues(collection.ForceCreatePPtr(lightingDataAsset));
					}
					else if (lightmapSettings.Has_LightmapSnapshot_C157())
					{
						lightmapSettings.LightmapSnapshot_C157.CopyValues(collection.ForceCreatePPtr(lightingDataAsset));
					}

					//Todo:
					//As described in AssetRipper#553, LightingDataAsset, LightProbes, and the lightmap textures
					//should all be exported in a subfolder beside the scene. This folder has the same name as the scene.
				}
			}
		}

		private static void CopyEnlightenSceneMapping(ILightingDataAsset lightingDataAsset, ILightmapSettings lightmapSettings)
		{
			if (!lightmapSettings.Has_EnlightenSceneMapping_C157())
			{
				return;
			}

			if (lightingDataAsset.EnlightenSceneMapping_C1120.Has_Probesets() && lightmapSettings.EnlightenSceneMapping_C157.Has_Probesets())
			{
				foreach (Hash128 hash in lightmapSettings.EnlightenSceneMapping_C157.Probesets)
				{
					lightingDataAsset.EnlightenSceneMapping_C1120.Probesets.AddNew().CopyValues(hash);
				}
			}

			foreach (EnlightenRendererInformation renderInfo in lightmapSettings.EnlightenSceneMapping_C157.Renderers)
			{
				EnlightenRendererInformation dataRenderInfo = lightingDataAsset.EnlightenSceneMapping_C1120.Renderers.AddNew();
				dataRenderInfo.DynamicLightmapSTInSystem.CopyValues(renderInfo.DynamicLightmapSTInSystem);
				dataRenderInfo.GeometryHash.CopyValues(renderInfo.GeometryHash);
				dataRenderInfo.InstanceHash.CopyValues(renderInfo.InstanceHash);
				SetPPtr(dataRenderInfo.Renderer, lightingDataAsset.Collection, renderInfo.Renderer, lightmapSettings.Collection);
				dataRenderInfo.SystemId = renderInfo.SystemId;
			}

			foreach (EnlightenSystemAtlasInformation atlasInfo in lightmapSettings.EnlightenSceneMapping_C157.SystemAtlases)
			{
				EnlightenSystemAtlasInformation dataAtlasInfo = lightingDataAsset.EnlightenSceneMapping_C1120.SystemAtlases.AddNew();
				dataAtlasInfo.AtlasHash.CopyValues(atlasInfo.AtlasHash);
				dataAtlasInfo.AtlasSize = atlasInfo.AtlasSize;
				dataAtlasInfo.FirstSystemId = atlasInfo.FirstSystemId;
			}

			foreach (EnlightenSystemInformation systemInfo in lightmapSettings.EnlightenSceneMapping_C157.Systems)
			{
				EnlightenSystemInformation dataSystemInfo = lightingDataAsset.EnlightenSceneMapping_C1120.Systems.AddNew();
				dataSystemInfo.AtlasIndex = systemInfo.AtlasIndex;
				dataSystemInfo.AtlasOffsetX = systemInfo.AtlasOffsetX;
				dataSystemInfo.AtlasOffsetY = systemInfo.AtlasOffsetY;
				dataSystemInfo.InputSystemHash.CopyValues(systemInfo.InputSystemHash);
				dataSystemInfo.RadiositySystemHash.CopyValues(systemInfo.RadiositySystemHash);
				dataSystemInfo.RendererIndex = systemInfo.RendererIndex;
				dataSystemInfo.RendererSize = systemInfo.RendererSize;
			}

			foreach (EnlightenTerrainChunksInformation terrainInfo in lightmapSettings.EnlightenSceneMapping_C157.TerrainChunks)
			{
				EnlightenTerrainChunksInformation dataTerrainInfo = lightingDataAsset.EnlightenSceneMapping_C1120.TerrainChunks.AddNew();
				dataTerrainInfo.FirstSystemId = terrainInfo.FirstSystemId;
				dataTerrainInfo.NumChunksInX = terrainInfo.NumChunksInX;
				dataTerrainInfo.NumChunksInY = terrainInfo.NumChunksInY;
			}
		}

		private static ProcessedAssetCollection CreateProcessedCollection(GameBundle gameBundle, UnityVersion projectVersion)
		{
			ProcessedAssetCollection processedCollection = new ProcessedAssetCollection(gameBundle);
			processedCollection.Name = "GeneratedLightingDataAssets";
			processedCollection.SetLayout(projectVersion, IO.Files.BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
			return processedCollection;
		}

		private static void SetPPtr<T>(IPPtr<T>? destination, AssetCollection destinationCollection, IPPtr<T>? source, AssetCollection sourceCollection)
			where T : IUnityObjectBase
		{
			destination?.CopyValues(destinationCollection.ForceCreatePPtr(sourceCollection.TryGetAsset(source?.ToStruct() ?? default)));
		}

		private static ILightingDataAsset CreateLightingDataAsset(ProcessedAssetCollection collection)
		{
			return collection.CreateAsset((int)ClassIDType.LightingDataAsset, LightingDataAssetFactory.CreateAsset);
		}
	}
}
