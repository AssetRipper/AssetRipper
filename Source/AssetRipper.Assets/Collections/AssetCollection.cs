using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using System.Collections;

namespace AssetRipper.Assets.Collections;

/// <summary>
/// A collection of <see cref="IUnityObjectBase"/> assets.
/// </summary>
public abstract class AssetCollection : IReadOnlyCollection<IUnityObjectBase>
{
	protected AssetCollection(Bundle bundle)
	{
		dependencies.Add(this);
		Bundle = bundle;
		bundle.AddCollection(this);
	}

	public Bundle Bundle { get; }
	public string Name { get; protected set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	/// <summary>
	/// The list of dependencies for this collection.
	/// </summary>
	/// <remarks>
	/// The zeroth entry is <see langword="this"/> for correct correspondence with file indices.
	/// Entries are null if they could not be found.
	/// </remarks>
	public IReadOnlyList<AssetCollection?> Dependencies => dependencies;
	private readonly List<AssetCollection?> dependencies = new();
	public IReadOnlyDictionary<long, IUnityObjectBase> Assets => assets;
	private readonly Dictionary<long, IUnityObjectBase> assets = new();
	public UnityVersion OriginalVersion { get; protected set; }
	public UnityVersion Version { get; protected set; }
	public BuildTarget Platform { get; protected set; }
	public TransferInstructionFlags Flags { get; protected set; }
	public EndianType EndianType { get; protected set; }

	[MemberNotNullWhen(true, nameof(Scene))]
	public bool IsScene => Scene is not null;

	public SceneDefinition? Scene { get; internal set; }

	public int AddDependency(AssetCollection dependency)
	{
		int index = dependencies.IndexOf(dependency);
		if (index >= 0)
		{
			return index;
		}
		else if (IsCompatibleDependency(dependency))
		{
			dependencies.Add(dependency);
			return dependencies.Count - 1;
		}
		else
		{
			throw new ArgumentException($"Dependency is not compatible with this {nameof(AssetCollection)}.", nameof(dependency));
		}
	}

	protected void SetDependency(int index, AssetCollection? collection)
	{
		if (index < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}
		else if (index < dependencies.Count)
		{
			dependencies[index] = collection;
		}
		else
		{
			while (dependencies.Count < index)
			{
				dependencies.Add(null);
			}
			dependencies.Add(collection);
		}
	}

	/// <summary>
	/// Determines if the given dependency collection is referencable from this collection.
	/// </summary>
	/// <param name="dependency"></param>
	/// <returns></returns>
	protected virtual bool IsCompatibleDependency(AssetCollection dependency) => true;

	public PPtr<T> CreatePPtr<T>(T? asset) where T : IUnityObjectBase
	{
		if (asset is null)
		{
			return default;
		}

		int fileIndex = dependencies.IndexOf(asset.Collection);
		if (fileIndex < 0)
		{
			throw new ArgumentException($"Asset doesn't belong to this {nameof(AssetCollection)} or any of its dependencies", nameof(asset));
		}
		return new PPtr<T>(fileIndex, asset.PathID);
	}

	public PPtr<T> ForceCreatePPtr<T>(T? asset) where T : IUnityObjectBase
	{
		if (asset is null)
		{
			return default;
		}

		int fileIndex = AddDependency(asset.Collection);
		return new PPtr<T>(fileIndex, asset.PathID);
	}

	protected void AddAsset(IUnityObjectBase asset)
	{
		ValidateAsset(asset);

		assets.Add(asset.PathID, asset);

		void ValidateAsset(IUnityObjectBase asset)
		{
			if (asset.Collection != this)
			{
				throw new ArgumentException("AssetInfo must have this marked as its collection.", nameof(asset));
			}
			if (asset.PathID is 0)
			{
				throw new ArgumentException("The zero path ID is reserved for null PPtr's.", nameof(asset));
			}
		}
	}

	/// <summary>
	/// Replace an asset in this collection.
	/// </summary>
	/// <remarks>
	/// This is useful for switching the underlying implementation, such as for version changing.
	/// </remarks>
	/// <param name="replacement"></param>
	public void ReplaceAsset(IUnityObjectBase replacement)
	{
		ValidateAsset(replacement);
		assets[replacement.PathID] = replacement;

		void ValidateAsset(IUnityObjectBase replacement)
		{
			if (replacement.Collection != this)
			{
				throw new ArgumentException("AssetInfo must have this marked as its collection.", nameof(replacement));
			}
			if (!assets.TryGetValue(replacement.PathID, out IUnityObjectBase? original))
			{
				throw new ArgumentException("There is no existing asset with this PathID.", nameof(replacement));
			}
			if (replacement.ClassID != original.ClassID)
			{
				throw new ArgumentException("The replacement asset's class id is not equal to the original asset's class id.", nameof(replacement));
			}
		}
	}

	public override string ToString()
	{
		return Name;
	}

	#region GetAsset Methods
	public IUnityObjectBase? TryGetAsset(long pathID)
	{
		TryGetAsset(pathID, out IUnityObjectBase? asset);
		return asset;
	}

	public T? TryGetAsset<T>(long pathID) where T : IUnityObjectBase
	{
		TryGetAsset(pathID, out T? asset);
		return asset;
	}

	public bool TryGetAsset(long pathID, [NotNullWhen(true)] out IUnityObjectBase? asset)
	{
		return TryGetAsset<IUnityObjectBase>(pathID, out asset);
	}

	public bool TryGetAsset<T>(long pathID, [NotNullWhen(true)] out T? asset) where T : IUnityObjectBase
	{
		if (assets.TryGetValue(pathID, out IUnityObjectBase? @object))
		{
			if (typeof(T).IsAssignableTo(typeof(NullObject)))
			{
				//T inherits from NullObject, so we allow the null object to be found.
				switch (@object)
				{
					case T t:
						asset = t;
						return true;
					default:
						asset = default;
						return false;
				}
			}
			else
			{
				switch (@object)
				{
					case NullObject:
						asset = default;
						return false;
					case T t:
						asset = t;
						return true;
					default:
						asset = default;
						return false;
				}
			}
		}
		else
		{
			asset = default;
			return false;
		}
	}

	public IUnityObjectBase? TryGetAsset(int fileIndex, long pathID)
	{
		TryGetAsset(fileIndex, pathID, out IUnityObjectBase? asset);
		return asset;
	}

	public T? TryGetAsset<T>(int fileIndex, long pathID) where T : IUnityObjectBase
	{
		TryGetAsset(fileIndex, pathID, out T? asset);
		return asset;
	}

	public bool TryGetAsset(int fileIndex, long pathID, [NotNullWhen(true)] out IUnityObjectBase? asset)
	{
		AssetCollection? file = TryGetDependency(fileIndex);
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

	public bool TryGetAsset<T>(int fileIndex, long pathID, [NotNullWhen(true)] out T? asset) where T : IUnityObjectBase
	{
		AssetCollection? file = TryGetDependency(fileIndex);
		if (file is not null)
		{
			return file.TryGetAsset(pathID, out asset);
		}
		else
		{
			asset = default;
			return false;
		}
	}

	public IUnityObjectBase? TryGetAsset(PPtr pptr) => TryGetAsset(pptr.FileID, pptr.PathID);

	public T? TryGetAsset<T>(PPtr<T> pptr) where T : IUnityObjectBase
	{
		return TryGetAsset<T>(pptr.FileID, pptr.PathID);
	}

	public bool TryGetAsset(PPtr pptr, [NotNullWhen(true)] out IUnityObjectBase? asset) => TryGetAsset(pptr.FileID, pptr.PathID, out asset);

	public bool TryGetAsset<T>(PPtr<T> pptr, [NotNullWhen(true)] out T? asset) where T : IUnityObjectBase
	{
		return TryGetAsset(pptr.FileID, pptr.PathID, out asset);
	}

	private AssetCollection? TryGetDependency(int fileIndex)
	{
		if (fileIndex < 0 || fileIndex >= Dependencies.Count)
		{
			return null;
		}
		else
		{
			return Dependencies[fileIndex];
		}
	}
	#endregion

	#region IReadOnlyCollection
	public IEnumerator<IUnityObjectBase> GetEnumerator() => assets.Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public int Count => assets.Count;
	#endregion
}
