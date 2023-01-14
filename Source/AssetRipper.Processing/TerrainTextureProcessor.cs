using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing
{
	public class TerrainTextureProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Terrain Alpha Texture Pairing");
			foreach (ITerrainData terrainData in gameBundle.FetchAssetsInHierarchy().OfType<ITerrainData>())
			{
				terrainData.MainAsset = terrainData;
				foreach (ITexture2D alphaTexture in terrainData.SplatDatabase_C156.AlphaTextures
					.Select(ptr => ptr.TryGetAsset(terrainData.Collection)).WhereNotNull())
				{
					alphaTexture.MainAsset = terrainData;
				}
			}
		}
	}
}
