using AssetRipper.Assets;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_27;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing;

public class MainAssetProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Main Asset Pairing");
		foreach (IUnityObjectBase asset in gameData.GameBundle.FetchAssets())
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
						if (font.TryGetFontTexture(out ITexture? fontTexture))
						{
							fontTexture.MainAsset = font;
						}
					}
					break;
				case ITerrainData terrainData:
					{
						terrainData.MainAsset = terrainData;
						foreach (ITexture2D alphaTexture in terrainData.GetSplatAlphaTextures())
						{
							//Sometimes TerrainData can be duplicated, but retain the same alpha textures.
							//https://github.com/AssetRipper/AssetRipper/issues/1356
							alphaTexture.MainAsset ??= terrainData;
						}
					}
					break;
			}
		}
	}
}
