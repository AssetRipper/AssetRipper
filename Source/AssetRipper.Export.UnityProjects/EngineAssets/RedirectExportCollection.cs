using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Export.UnityProjects.EngineAssets;

public class RedirectExportCollection : IExportCollection
{
	private readonly Dictionary<IUnityObjectBase, (long, UnityGuid, AssetType)> redirectionDictionary = new();
	private readonly Dictionary<IUnityObjectBase, AssetType> missingDictionary = new();

	public IEnumerable<IUnityObjectBase> Assets
	{
		get
		{
			if (missingDictionary.Count == 0)
			{
				return redirectionDictionary.Keys;
			}
			else if (redirectionDictionary.Count == 0)
			{
				return missingDictionary.Keys;
			}
			else
			{
				return redirectionDictionary.Keys.Concat(missingDictionary.Keys);
			}
		}
	}

	public string Name => redirectionDictionary.Count == 1 ? redirectionDictionary.Keys.First().GetBestName() : nameof(RedirectExportCollection);

	public void Add(IUnityObjectBase asset, long exportID, UnityGuid guid, AssetType assetType)
	{
		ValidateArguments(exportID, guid);
		redirectionDictionary.Add(asset, (exportID, guid, assetType));

		static void ValidateArguments(long exportID, UnityGuid guid)
		{
			if (exportID == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(exportID), "The export id must be nonzero.");
			}
			if (guid.IsZero || guid == UnityGuid.MissingReference)
			{
				throw new ArgumentException("The guid cannot be zero or missing.", nameof(guid));
			}
		}
	}

	public void AddMissing(IUnityObjectBase asset, AssetType assetType)
	{
		missingDictionary.Add(asset, assetType);
	}

	public MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		if (isLocal)
		{
			throw new NotSupportedException();
		}

		if (missingDictionary.TryGetValue(asset, out AssetType missingAssetType))
		{
			return MetaPtr.CreateMissingReference(asset.ClassID, missingAssetType);
		}

		(long exportID, UnityGuid guid, AssetType assetType) = redirectionDictionary[asset];
		return new MetaPtr(exportID, guid, assetType);
	}

	public long GetExportID(IExportContainer container, IUnityObjectBase asset) => redirectionDictionary[asset].Item1;

	public bool Contains(IUnityObjectBase asset) => redirectionDictionary.ContainsKey(asset) || missingDictionary.ContainsKey(asset);

	bool IExportCollection.Export(IExportContainer container, string projectDirectory)
	{
		return true; //successfully redirected
	}

	AssetCollection IExportCollection.File => throw new NotSupportedException();

	TransferInstructionFlags IExportCollection.Flags => throw new NotSupportedException();
}
