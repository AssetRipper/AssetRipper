using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of <see cref="IUnityObjectBase"/> assets.
/// </summary>
public abstract class AssetCollection : IReadOnlyCollection<IUnityObjectBase>
{
	protected AssetCollection(Bundle bundle)
	{
		Bundle = bundle;
		dependencies.Add(this);
	}

	public Bundle Bundle { get; }

	/// <summary>
	/// The list of dependencies for this collection.
	/// </summary>
	/// <remarks>
	/// The zeroth entry is <see langword="this"/> for correct correspondence with file indices.
	/// Entries are null if they could not be found.
	/// </remarks>
	public IReadOnlyList<AssetCollection?> Dependencies => dependencies;
	private readonly List<AssetCollection?> dependencies = new();
	private readonly Dictionary<long, IUnityObjectBase> assets = new();

	public int AddDependency(AssetCollection dependency)
	{
		int index = dependencies.IndexOf(dependency);
		if (index >= 0)
		{
			return index;
		}
		else if (IsValidDependency(dependency))
		{
			dependencies.Add(dependency);
			return dependencies.Count - 1;
		}
		else
		{
			throw new ArgumentException($"Dependency is not valid for this {nameof(AssetCollection)}", nameof(dependency));
		}
	}

	protected virtual bool IsValidDependency(AssetCollection dependency) => true;

	public PPtr CreatePPtr(IUnityObjectBase asset)
	{
		int fileIndex = dependencies.IndexOf(asset.Collection);
		if (fileIndex < 0)
		{
			throw new Exception($"Asset doesn't belong to this {nameof(AssetCollection)} or any of its dependencies");
		}
		return new PPtr(fileIndex, asset.PathID);
	}

	public PPtr ForceCreatePPtr(IUnityObjectBase asset)
	{
		int fileIndex = AddDependency(asset.Collection);
		return new PPtr(fileIndex, asset.PathID);
	}

	private protected void AddAsset(IUnityObjectBase asset)
	{
		Debug.Assert(asset.Collection == this, "Asset info must marked this as its collection");
		assets.Add(asset.PathID, asset);
	}

	#region GetAsset Methods
	public IUnityObjectBase GetAsset(long pathID)
	{
		return TryGetAsset(pathID, out IUnityObjectBase? asset)
			? asset
			: throw new Exception($"Object with path ID {pathID} wasn't found");
	}

	public IUnityObjectBase? TryGetAsset(long pathID)
	{
		TryGetAsset(pathID, out IUnityObjectBase? asset);
		return asset;
	}

	public bool TryGetAsset(long pathID, [NotNullWhen(true)] out IUnityObjectBase? asset)
	{
		return assets.TryGetValue(pathID, out asset);
	}

	public IUnityObjectBase GetAsset(int fileIndex, long pathID)
	{
		ThrowIfFileIndexOutOfRange(fileIndex);
		AssetCollection file = Dependencies[fileIndex] ?? throw new Exception($"Dependency collection with index {fileIndex} was not found.");
		return file.GetAsset(pathID);
	}

	public IUnityObjectBase? TryGetAsset(int fileIndex, long pathID)
	{
		TryGetAsset(fileIndex, pathID, out IUnityObjectBase? asset);
		return asset;
	}

	public bool TryGetAsset(int fileIndex, long pathID, [NotNullWhen(true)] out IUnityObjectBase? asset)
	{
		ThrowIfFileIndexOutOfRange(fileIndex);
		AssetCollection? file = Dependencies[fileIndex];
		if (file is not null)
		{
			return file.TryGetAsset(pathID, out asset);
		}
		else
		{
			asset = null;
			return false;
		}
	}

	public IUnityObjectBase GetAsset(PPtr pptr) => GetAsset(pptr.FileID, pptr.PathID);

	public IUnityObjectBase? TryGetAsset(PPtr pptr) => TryGetAsset(pptr.FileID, pptr.PathID);

	public bool TryGetAsset(PPtr pptr, [NotNullWhen(true)] out IUnityObjectBase? asset) => TryGetAsset(pptr.FileID, pptr.PathID, out asset);

	private void ThrowIfFileIndexOutOfRange(int fileIndex)
	{
		if (fileIndex < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(fileIndex), $"File index cannot be negative: {fileIndex}");
		}
		else if (fileIndex >= Dependencies.Count)
		{
			throw new ArgumentException($"{nameof(AssetCollection)} with index {fileIndex} was not found in dependencies", nameof(fileIndex));
		}
	}
	#endregion

	#region IReadOnlyCollection
	public IEnumerator<IUnityObjectBase> GetEnumerator() => assets.Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public int Count => assets.Count;
	#endregion
}
