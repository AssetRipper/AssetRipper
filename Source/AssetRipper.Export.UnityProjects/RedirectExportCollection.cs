using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.SerializedFiles;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects;

public sealed class RedirectExportCollection : IExportCollection
{
	private readonly Dictionary<IUnityObjectBase, MetaPtr> redirectionDictionary = new();
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
		ArgumentOutOfRangeException.ThrowIfEqual(exportID, 0);
		ArgumentOutOfRangeException.ThrowIfEqual(guid, UnityGuid.MissingReference);
		ArgumentOutOfRangeException.ThrowIfEqual(guid, UnityGuid.Zero);

		redirectionDictionary.Add(asset, new MetaPtr(exportID, guid, assetType));
	}

	public void AddMissing(IUnityObjectBase asset, AssetType assetType)
	{
		missingDictionary.Add(asset, assetType);
	}

	public MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		ThrowIfLocal(isLocal);

		if (missingDictionary.TryGetValue(asset, out AssetType missingAssetType))
		{
			return MetaPtr.CreateMissingReference(asset.ClassID, missingAssetType);
		}

		return redirectionDictionary[asset];

		[StackTraceHidden]
		static void ThrowIfLocal(bool isLocal)
		{
			if (isLocal)
			{
				throw new NotSupportedException();
			}
		}
	}

	public long GetExportID(IExportContainer container, IUnityObjectBase asset) => redirectionDictionary[asset].FileID;

	public bool Contains(IUnityObjectBase asset) => redirectionDictionary.ContainsKey(asset) || missingDictionary.ContainsKey(asset);

	bool IExportCollection.Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		throw new NotSupportedException();
	}

	bool IExportCollection.Exportable => false;

	AssetCollection IExportCollection.File => throw new NotSupportedException();

	TransferInstructionFlags IExportCollection.Flags => throw new NotSupportedException();
}
