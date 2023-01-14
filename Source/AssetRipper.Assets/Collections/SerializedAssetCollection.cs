using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of assets read from a <see cref="SerializedFile"/>.
/// </summary>
public class SerializedAssetCollection : AssetCollection
{
	public FileIdentifier[]? DependencyIdentifiers { get; private set; }

	protected SerializedAssetCollection(Bundle bundle) : base(bundle)
	{
	}

	protected override bool IsCompatibleDependency(AssetCollection dependency)
	{
		return dependency is SerializedAssetCollection or ProcessedAssetCollection;
	}

	public void InitializeDependencyList()
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

	public IEnumerable<FileIdentifier> GetUnresolvedDependencies()
	{
		if (DependencyIdentifiers is not null)
		{
			for (int i = 0; i < DependencyIdentifiers.Length; i++)
			{
				FileIdentifier identifier = DependencyIdentifiers[i];
				if (Bundle.ResolveCollection(identifier) is null)
				{
					yield return identifier;
				}
			}
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
		if (SerializedFileMetadata.HasScriptTypes(file.Header.Version))
		{
			for (int i = 0; i < file.Metadata.ScriptTypes.Length; i++)
			{
				LocalSerializedObjectIdentifier ptr = file.Metadata.ScriptTypes[i];
				if (ptr.LocalSerializedFileIndex == 0)
				{
					int index = file.AssetEntryLookup[ptr.LocalIdentifierInFile];
					ObjectInfo objectInfo = file.Metadata.Object[index];
					ReadAsset(collection, file, objectInfo, factory);
				}
			}
		}

		for (int i = 0; i < file.Metadata.Object.Length; i++)
		{
			ObjectInfo objectInfo = file.Metadata.Object[i];
			if (objectInfo.ClassID == 115)//MonoScript
			{
				if (!collection.Assets.ContainsKey(objectInfo.FileID))
				{
					ReadAsset(collection, file, objectInfo, factory);
				}
			}
		}

		for (int i = 0; i < file.Metadata.Object.Length; i++)
		{
			ObjectInfo objectInfo = file.Metadata.Object[i];
			if (!collection.Assets.ContainsKey(objectInfo.FileID))
			{
				ReadAsset(collection, file, objectInfo, factory);
			}
		}
	}

	private static void ReadAsset(SerializedAssetCollection collection, SerializedFile file, ObjectInfo info, AssetFactoryBase factory)
	{
		SerializedType? type = info.GetSerializedType(file.Metadata.Types);
		int classID = info.TypeID < 0 ? 114 : info.TypeID;
		AssetInfo assetInfo = new AssetInfo(collection, info.FileID, classID);
		using MemoryStream memoryStream = new MemoryStream(info.ObjectData, false);
		using AssetReader reader = new AssetReader(memoryStream, collection);
		IUnityObjectBase? asset = factory.ReadAsset(assetInfo, reader, info.ObjectData.Length, type);
		if (asset is not null)
		{
			collection.AddAsset(asset);
		}
	}
}
