using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A container for <see cref="AssetCollection"/>s, <see cref="ResourceFile"/>s, and other <see cref="Bundle"/>s.
/// </summary>
public abstract class Bundle : IDisposable
{
	public Bundle? Parent { get; internal set; }
	public IReadOnlyList<ResourceFile> Resources => resources;
	private readonly List<ResourceFile> resources = new();
	public IReadOnlyList<AssetCollection> Collections => collections;
	private readonly List<AssetCollection> collections = new();
	public IReadOnlyList<Bundle> Bundles => bundles;
	private readonly List<Bundle> bundles = new();
	private bool disposedValue;

	public abstract string Name { get; }
	
	public void InitializeAllDependencyLists()
	{
		foreach (AssetCollection collection in Collections)
		{
			if (collection is SerializedAssetCollection serializedAssetCollection)
			{
				serializedAssetCollection.InitializeDependencyList();
			}
		}
		foreach (Bundle bundle in Bundles)
		{
			bundle.InitializeAllDependencyLists();
		}
	}

	public AssetCollection? ResolveCollection(FileIdentifier identifier)
	{
		return ResolveCollection(identifier.GetFilePath());
	}

	public virtual AssetCollection? ResolveCollection(string name)
	{
		Bundle? bundleToExclude = null;
		Bundle? currentBundle = this;
		while (currentBundle is not null)
		{
			AssetCollection? result = TryResolveFromCollections(currentBundle, name) ?? TryResolveFromChildBundles(currentBundle, name, bundleToExclude);
			if (result is not null)
			{
				return result;
			}

			bundleToExclude = currentBundle;
			currentBundle = currentBundle.Parent;
		}

		return null;

		static AssetCollection? TryResolveFromCollections(Bundle currentBundle, string name)
		{
			//Uniqueness is not guaranteed because of asset bundle variants
			return currentBundle.Collections.FirstOrDefault(c => c.Name == name);
		}

		static AssetCollection? TryResolveFromChildBundles(Bundle currentBundle, string name, Bundle? bundleToExclude)
		{
			return currentBundle.Bundles
				.Where(b => b != bundleToExclude)
				.Select(b => TryResolveFromCollections(b, name))//This completely ignores that b might have overrided resolution
				.FirstOrDefault(c => c is not null);
		}
	}

	public ResourceFile? ResolveResource([NotNullWhen(true)] string? name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}

		string fixedName = FilenameUtils.FixResourcePath(name);
		return ResolveResourceInternal(name, fixedName);
	}

	protected virtual ResourceFile? ResolveResourceInternal(string originalName, string fixedName)
	{
		Bundle? bundleToExclude = null;
		Bundle? currentBundle = this;
		while (currentBundle is not null)
		{
			ResourceFile? result = TryResolveFromResources(currentBundle, fixedName) ?? TryResolveFromChildBundles(currentBundle, originalName, fixedName, bundleToExclude);
			if (result is not null)
			{
				return result;
			}

			bundleToExclude = currentBundle;
			currentBundle = currentBundle.Parent;
		}

		return null;

		static ResourceFile? TryResolveFromResources(Bundle currentBundle, string fixedName)
		{
			//Uniqueness is not guaranteed because of asset bundle variants
			return currentBundle.Resources.FirstOrDefault(c => c.Name == fixedName);
		}

		static ResourceFile? TryResolveFromChildBundles(Bundle currentBundle, string originalName, string fixedName, Bundle? bundleToExclude)
		{
			return currentBundle.Bundles
				.Where(b => b != bundleToExclude)
				.Select(b => TryResolveFromResources(b, fixedName))//This completely ignores that b might have overrided resolution
				.FirstOrDefault(r => r is not null);
		}
	}

	public void AddResource(ResourceFile resource)
	{
		resources.Add(resource);
	}

	public void AddCollection(AssetCollection collection)
	{
		if (collection.Bundle != this)
		{
			throw new ArgumentException($"Collection's {nameof(AssetCollection.Bundle)} property did not match this.", nameof(collection));
		}
		else if (IsCompatibleCollection(collection))
		{
			collections.Add(collection);
		}
		else
		{
			throw new ArgumentException($"The collection is not compatible with this {nameof(Bundle)}.", nameof(collection));
		}
	}

	public void AddBundle(Bundle bundle)
	{
		if (bundle.Parent is null)
		{
			if (IsCompatibleBundle(bundle))
			{
				bundles.Add(bundle);
				bundle.Parent = this;
			}
			else
			{
				throw new ArgumentException($"Child {nameof(Bundle)} is not compatible with this parent {nameof(Bundle)}.", nameof(bundle));
			}
		}
		else if (bundle.Parent == this)
		{
		}
		else
		{
			throw new ArgumentException($"{nameof(bundle)} already has a parent.", nameof(bundle));
		}
	}

	protected virtual bool IsCompatibleCollection(AssetCollection collection) => true;

	protected virtual bool IsCompatibleBundle(Bundle bundle) => bundle is not GameBundle;

	public Bundle GetRoot()
	{
		Bundle root = this;
		while (root.Parent is not null)
		{
			root = root.Parent;
		}
		return root;
	}

	public IEnumerable<IUnityObjectBase> FetchAssetsInHierarchy()
	{
		return GetRoot().FetchAssets();
	}

	public IEnumerable<IUnityObjectBase> FetchAssets()
	{
		foreach (AssetCollection collection in collections)
		{
			foreach (IUnityObjectBase asset in collection)
			{
				yield return asset;
			}
		}
		foreach (Bundle bundle in bundles)
		{
			foreach (IUnityObjectBase asset in bundle.FetchAssets())
			{
				yield return asset;
			}
		}
	}

	public IEnumerable<AssetCollection> FetchAssetCollections()
	{
		foreach (AssetCollection collection in collections)
		{
			yield return collection;
		}
		foreach (Bundle bundle in bundles)
		{
			foreach (AssetCollection collection in bundle.FetchAssetCollections())
			{
				yield return collection;
			}
		}
	}

	public IEnumerable<ResourceFile> FetchResourceFiles()
	{
		foreach (ResourceFile resource in resources)
		{
			yield return resource;
		}
		foreach (Bundle bundle in bundles)
		{
			foreach (ResourceFile resource in bundle.FetchResourceFiles())
			{
				yield return resource;
			}
		}
	}

	public IEnumerable<FileIdentifier> GetUnresolvedDependencies()
	{
		foreach (AssetCollection collection in collections)
		{
			if (collection is SerializedAssetCollection serializedCollection)
			{
				foreach (FileIdentifier identifier in serializedCollection.GetUnresolvedDependencies())
				{
					yield return identifier;
				}
			}
		}
		foreach (Bundle bundle in bundles)
		{
			foreach (FileIdentifier identifier in bundle.GetUnresolvedDependencies())
			{
				yield return identifier;
			}
		}
	}

	public override string ToString()
	{
		return Name;
	}

	public SerializedAssetCollection AddCollectionFromSerializedFile(SerializedFile file, AssetFactoryBase factory)
	{
		return SerializedAssetCollection.FromSerializedFile(this, file, factory);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				foreach (ResourceFile resourceFile in resources)
				{
					resourceFile.Dispose();
				}
				foreach (Bundle bundle in bundles)
				{
					bundle.Dispose();
				}
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
