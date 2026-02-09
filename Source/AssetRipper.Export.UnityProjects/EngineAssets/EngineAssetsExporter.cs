using AssetRipper.Assets;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.Tpk;
using AssetRipper.Tpk.EngineAssets;
using AssetType = AssetRipper.IO.Files.AssetType;

namespace AssetRipper.Export.UnityProjects.EngineAssets;

public class EngineAssetsExporter : IAssetExporter
{
	private static Utf8String FontMaterialName { get; } = (Utf8String)"Font Material"u8;
	private static Utf8String FontTextureName { get; } = (Utf8String)"Font Texture"u8;

	private PredefinedAssetCache Cache { get; }

	public EngineAssetsExporter(PredefinedAssetCache cache)
	{
		Cache = cache;
	}

	public static EngineAssetsExporter CreateFromEmbeddedData(UnityVersion version) => CreateFromTpkFile(version, EngineAssetsTpk.GetStream());

	public static EngineAssetsExporter CreateFromTpkFile(UnityVersion version, Stream stream)
	{
		TpkFile tpk = TpkFile.FromStream(stream);
		return CreateFromTpkBlob(version, (TpkEngineAssetsBlob)tpk.GetDataBlob());
	}

	public static EngineAssetsExporter CreateFromTpkBlob(UnityVersion version, TpkEngineAssetsBlob blob)
	{
		if (blob.Data.Count == 0)
		{
			return new(new PredefinedAssetCache());
		}

		int index;
		if (version <= blob.Data[0].Key)
		{
			index = 0;
		}
		else
		{
			index = blob.Data.Count - 1;
			for (int i = 1; i < blob.Data.Count; i++)
			{
				if (version <= blob.Data[i].Key)
				{
					index = i - 1;
					break;
				}
			}
		}

		string json = blob.Data[index].Value;
		return CreateFromJsonText(json);
	}

	public static EngineAssetsExporter CreateFromJsonText(string json)
	{
		return CreateFromResourceData(EngineResourceData.FromJson(json));
	}

	public static EngineAssetsExporter CreateFromResourceData(EngineResourceData resourceData)
	{
		return new(new PredefinedAssetCache(resourceData));
	}

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (IsEngineFile(asset.Collection.Name, out UnityGuid engineGuid))
		{
			exportCollection = new SingleRedirectExportCollection(asset, asset.PathID, engineGuid, AssetType.Internal);
			return true;
		}
		else
		{
			if (asset is IMaterial material)
			{
				if (material.Name == FontMaterialName)
				{
					goto returnFalse;
				}
			}
			else if (asset is ITexture2D texture)
			{
				if (texture.Name == FontTextureName)
				{
					goto returnFalse;
				}
			}
		}

		if (asset.MainAsset is SpriteInformationObject spriteInformationObject)
		{
			if (Cache.Contains(spriteInformationObject.Texture, out long textureID, out UnityGuid textureGuid, out _))
			{
				RedirectExportCollection redirectExportCollection = new();
				redirectExportCollection.Add(spriteInformationObject.Texture, textureID, textureGuid, AssetType.Internal);
				redirectExportCollection.AddMissing(spriteInformationObject, default);
				exportCollection = redirectExportCollection;

				//I think it's safe to ignore sprite atlases.
				foreach ((ISprite sprite, _) in spriteInformationObject.Sprites)
				{
					if (Cache.Contains(sprite, out long spriteID, out UnityGuid spriteGuid, out _))
					{
						redirectExportCollection.Add(sprite, spriteID, spriteGuid, AssetType.Internal);
					}
					else
					{
						redirectExportCollection.AddMissing(sprite, AssetType.Internal);
					}
				}
				return true;
			}
		}
		else if (Cache.Contains(asset, out long exportID, out UnityGuid guid, out _))
		{
			exportCollection = new SingleRedirectExportCollection(asset, exportID, guid, AssetType.Internal);
			return true;
		}

		returnFalse:
		exportCollection = null;
		return false;
	}

	private static bool IsEngineFile(string? fileName, out UnityGuid guid)
	{
		if (SpecialFileNames.IsDefaultResource(fileName))
		{
			guid = PredefinedAssetCache.EGUID;
			return true;
		}
		else if (SpecialFileNames.IsBuiltinExtra(fileName))
		{
			guid = PredefinedAssetCache.FGUID;
			return true;
		}
		guid = default;
		return false;
	}

	public AssetType ToExportType(IUnityObjectBase asset)
	{
		return AssetType.Internal;
	}

	public bool ToUnknownExportType(Type type, out AssetType assetType)
	{
		assetType = AssetType.Internal;
		return false;
	}
}
