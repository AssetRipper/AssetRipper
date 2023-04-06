using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of assets read from a <see cref="SerializedFile"/>.
/// </summary>
public sealed class SerializedAssetCollection : AssetCollection
{
	public FileIdentifier[]? DependencyIdentifiers { get; private set; }

	private SerializedAssetCollection(Bundle bundle) : base(bundle)
	{
	}

	protected override bool IsCompatibleDependency(AssetCollection dependency)
	{
		return dependency is SerializedAssetCollection or ProcessedAssetCollection;
	}

	internal void InitializeDependencyList()
	{
		if (Dependencies.Count > 1)
		{
			throw new Exception("Dependency list has already been initialized.");
		}
		if (DependencyIdentifiers is not null)
		{
			for (int i = 0; i < DependencyIdentifiers.Length; i++)
			{
				FileIdentifier identifier = DependencyIdentifiers[i];
				SetDependency(i + 1, Bundle.ResolveCollection(identifier));
			}
			DependencyIdentifiers = null;
		}
	}

	internal static SerializedAssetCollection FromSerializedFile(Bundle bundle, SerializedFile file, AssetFactoryBase factory)
	{
		SerializedAssetCollection collection = new SerializedAssetCollection(bundle)
		{
			Name = file.NameFixed,
			Version = file.Version,
			Platform = file.Platform,
			Flags = file.Flags,
			EndianType = file.EndianType,
		};
		FileIdentifier[] fileDependencies = file.Metadata.Externals;
		if (fileDependencies.Length > 0)
		{
			collection.DependencyIdentifiers = new FileIdentifier[fileDependencies.Length];
			Array.Copy(fileDependencies, collection.DependencyIdentifiers, fileDependencies.Length);
		}
		ReadData(collection, file, factory);
		return collection;
	}

	private static void ReadData(SerializedAssetCollection collection, SerializedFile file, AssetFactoryBase factory)
	{
		for (int i = 0; i < file.Metadata.Object.Length; i++)
		{
			ObjectInfo objectInfo = file.Metadata.Object[i];
			SerializedType? type = objectInfo.GetSerializedType(file.Metadata.Types);
			int classID = objectInfo.TypeID < 0 ? 114 : objectInfo.TypeID;
			AssetInfo assetInfo = new AssetInfo(collection, objectInfo.FileID, classID);
			IUnityObjectBase? asset = factory.ReadAsset(assetInfo, objectInfo.ObjectData, type);
			if (asset is not null)
			{
				collection.AddAsset(asset);
			}
		}
	}
}
