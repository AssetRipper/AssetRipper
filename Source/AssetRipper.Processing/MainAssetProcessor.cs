using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing
{
	public class MainAssetProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Main Asset Pairing");
			foreach (IUnityObjectBase asset in gameBundle.FetchAssetsInHierarchy())
			{
				switch (asset)
				{
					case IFont font:
						{
							font.MainAsset = font;
							if (font.TryGetFontMaterial(out IMaterial? fontMaterial))
							{
								fontMaterial.MainAsset = font;
							}
						}
						break;
					case ITerrainData terrainData:
						{
							terrainData.MainAsset = terrainData;
							foreach (ITexture2D alphaTexture in GetSplatAlphaTextures(terrainData))
							{
								alphaTexture.MainAsset = terrainData;
							}
						}
						break;
				}
			}
		}

		private static IEnumerable<ITexture2D> GetSplatAlphaTextures(ITerrainData terrainData)
		{
			return terrainData.SplatDatabase_C156.AlphaTextures.Select(ptr => ptr.TryGetAsset(terrainData.Collection)).WhereNotNull();
		}
	}
}
