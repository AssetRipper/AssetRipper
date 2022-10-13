using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Linq;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using System.Linq;

namespace AssetRipper.Library.Processors
{
	public class TerrainTextureProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Terrain Alpha Texture Pairing");
			foreach (ITerrainData terrainData in gameBundle.FetchAssetsInHierarchy().SelectType<IUnityObjectBase, ITerrainData>())
			{
				foreach (ITexture2D alphaTexture in terrainData.SplatDatabase_C156.AlphaTextures
					.Select(ptr => ptr.TryGetAsset(terrainData.Collection)).WhereNotNull())
				{
					alphaTexture.TerrainData = terrainData;
				}
			}
		}
	}
}
