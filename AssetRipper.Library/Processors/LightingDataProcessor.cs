using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Linq;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1120;
using AssetRipper.SourceGenerated.Classes.ClassID_157;
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
				if (lightmapSettings is not null && lightmapSettings.Has_LightingDataAsset_C157())
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

					lightmapSettings.LightingDataAsset_C157.CopyValues(collection.ForceCreatePPtr(lightingDataAsset));

					//Todo:
					//As described in AssetRipper#553, LightingDataAsset, LightProbes, and the lightmap textures
					//should all be exported in a subfolder beside the scene. This folder has the same name as the scene.
				}
			}
		}

		private static ProcessedAssetCollection CreateProcessedCollection(GameBundle gameBundle, UnityVersion projectVersion)
		{
			ProcessedAssetCollection processedCollection = new ProcessedAssetCollection(gameBundle);
			processedCollection.Name = "GeneratedLightingDataAssets";
			processedCollection.SetLayout(projectVersion, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
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
