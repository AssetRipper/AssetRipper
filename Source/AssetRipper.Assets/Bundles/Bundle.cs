using AssetRipper.Assets.Collections;
using AssetRipper.Assets.IO;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Bundles;

/// <summary>
/// A container for <see cref="AssetCollection"/>s, <see cref="ResourceFile"/>s, and other <see cref="Bundle"/>s.
/// </summary>
public abstract class Bundle : IDisposable
{
	/// <summary>
	/// The parent <see cref="Bundle"/> of this Bundle.
	/// </summary>
	public Bundle? Parent { get; private set; }

	/// <summary>
	/// The list of <see cref="ResourceFile"/>s in this Bundle.
	/// </summary>
	public IReadOnlyList<ResourceFile> Resources => resources;
	private readonly List<ResourceFile> resources = [];

	/// <summary>
	/// The list of <see cref="AssetCollection"/>s in this Bundle.
	/// </summary>
	public IReadOnlyList<AssetCollection> Collections => collections;
	private readonly List<AssetCollection> collections = [];

	/// <summary>
	/// The list of child <see cref="Bundle"/>s in this Bundle.
	/// </summary>
	public IReadOnlyList<Bundle> Bundles => bundles;
	private readonly List<Bundle> bundles = [];

	/// <summary>
	/// The list of <see cref="FailedFile"/>s in this Bundle.
	/// </summary>
	public IReadOnlyList<FailedFile> FailedFiles => failedFiles;
	private readonly List<FailedFile> failedFiles = [];

	private bool disposedValue;

	/// <summary>
	/// The name of this Bundle.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	/// All the <see cref="SceneDefinition"/>s in this bundle.
	/// </summary>
	public IEnumerable<SceneDefinition> Scenes
	{
		get
		{
			HashSet<SceneDefinition> scenes = new();
			foreach (AssetCollection collection in FetchAssetCollections())
			{
				SceneDefinition? scene = collection.Scene;
				if (scene is not null && scenes.Add(scene))
				{
					yield return scene;
				}
			}
		}
	}

	/// <summary>
	/// Initializes the dependency list for each SerializedAssetCollection in this Bundle and its children Bundles.
	/// </summary>
	internal void InitializeAllDependencyLists(IDependencyProvider? dependencyProvider)
	{
		foreach (AssetCollection collection in Collections)
		{
			if (collection is SerializedAssetCollection serializedAssetCollection)
			{
				serializedAssetCollection.InitializeDependencyList(dependencyProvider);
			}
		}
		foreach (Bundle bundle in Bundles)
		{
			bundle.InitializeAllDependencyLists(dependencyProvider);
		}
	}

	/// <summary>
	/// Resolves an <see cref="AssetCollection"/> with the specified name in this Bundle and its ascendants.
	/// </summary>
	/// <param name="identifier">The identifier of the file of the <see cref="AssetCollection"/>.</param>
	/// <returns>The resolved <see cref="AssetCollection"/> if it exists, else null.</returns>
	public AssetCollection? ResolveCollection(FileIdentifier identifier)
	{
		return ResolveCollection(identifier.GetFilePath());
	}

	/// <summary>
	/// Resolves an <see cref="AssetCollection"/> with the specified name in this Bundle and its ascendants.
	/// </summary>
	/// <param name="name">The name of the <see cref="AssetCollection"/>.</param>
	/// <returns>The resolved <see cref="AssetCollection"/> if it exists, else null.</returns>
	public AssetCollection? ResolveCollection(string name)
	{
		AssetCollection? result = ResolveInternal(name);
		if (result is not null)
		{
			return result;
		}

		string fixedName = SpecialFileNames.FixFileIdentifier(name);
		result = ResolveInternal(fixedName);
		if (result is not null)
		{
			return result;
		}

		return fixedName switch
		{
			SpecialFileNames.DefaultResourceName1 => ResolveInternal(SpecialFileNames.DefaultResourceName2),
			SpecialFileNames.DefaultResourceName2 => ResolveInternal(SpecialFileNames.DefaultResourceName1),
			SpecialFileNames.BuiltinExtraName1 => ResolveInternal(SpecialFileNames.BuiltinExtraName2),
			SpecialFileNames.BuiltinExtraName2 => ResolveInternal(SpecialFileNames.BuiltinExtraName1),
			_ => null,
		};

		AssetCollection? ResolveInternal(string name)
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
		}

		/// <summary>
		/// Attempts to resolve an <see cref="AssetCollection"/> with the specified name in the specified Bundle's collections.
		/// </summary>
		/// <param name="currentBundle">The Bundle to attempt to resolve the <see cref="AssetCollection"/> from.</param>
		/// <param name="name">The name of the <see cref="AssetCollection"/>.</param>
		/// <returns>The resolved <see cref="AssetCollection"/> if it exists, else null.</returns>
		static AssetCollection? TryResolveFromCollections(Bundle currentBundle, string name)
		{
			//Uniqueness is not guaranteed because of asset bundle variants
			foreach (AssetCollection collection in currentBundle.Collections)
			{
				if (collection.Name == name)
				{
					return collection;
				}
			}

			return null;
		}

		/// <summary>
		/// Attempts to resolve an <see cref="AssetCollection"/> with the specified name in the specified Bundle's child Bundles.
		/// </summary>
		/// <param name="currentBundle">The Bundle to attempt to resolve the <see cref="AssetCollection"/> from.</param>
		/// <param name="name">The name of the <see cref="AssetCollection"/>.</param>
		/// <param name="bundleToExclude">The <see cref="Bundle"/> to exclude from the search.</param>
		/// <returns>The resolved <see cref="AssetCollection"/> if it exists, else null.</returns>
		static AssetCollection? TryResolveFromChildBundles(Bundle currentBundle, string name, Bundle? bundleToExclude)
		{
			foreach (Bundle bundle in currentBundle.Bundles)
			{
				if (bundle != bundleToExclude && TryResolveFromCollections(bundle, name) is { } collection)
				{
					return collection;
				}
			}

			return null;
		}
	}

	/// <summary>
	/// Resolves a ResourceFile with the specified name in this Bundle and its ascendants.
	/// </summary>
	/// <param name="name">The name of the ResourceFile.</param>
	/// <returns>The resolved ResourceFile if it exists, else null.</returns>
	public ResourceFile? ResolveResource([NotNullWhen(true)] string? name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}

		string originalName = name;
		string fixedName = SpecialFileNames.FixFileIdentifier(name);

		Bundle? bundleToExclude = null;
		Bundle? currentBundle = this;
		while (currentBundle is not null)
		{
			ResourceFile? result = TryResolveFromResources(currentBundle, fixedName)
				?? TryResolveFromChildBundles(currentBundle, originalName, fixedName, bundleToExclude)
				?? currentBundle.ResolveExternalResource(originalName);
			if (result is not null)
			{
				return result;
			}

			bundleToExclude = currentBundle;
			currentBundle = currentBundle.Parent;
		}

		return null;

		/// <summary>
		/// Attempts to resolve a ResourceFile with the specified name in the specified Bundle's Resources.
		/// </summary>
		/// <param name="currentBundle">The Bundle to attempt to resolve the ResourceFile from.</param>
		/// <param name="fixedName">The name of the ResourceFile with invalid characters and path separators fixed.</param>
		/// <returns>The resolved ResourceFile if it exists, else null.</returns>
		static ResourceFile? TryResolveFromResources(Bundle currentBundle, string fixedName)
		{
			//Uniqueness is not guaranteed because of asset bundle variants
			foreach (ResourceFile resource in currentBundle.Resources)
			{
				if (resource.NameFixed == fixedName)
				{
					return resource;
				}
			}

			return null;
		}

		/// <summary>
		/// Attempts to resolve a ResourceFile with the specified name in the specified Bundle's child Bundles.
		/// </summary>
		/// <param name="currentBundle">The Bundle to attempt to resolve the ResourceFile from.</param>
		/// <param name="originalName">The original name of the ResourceFile.</param>
		/// <param name="fixedName">The name of the ResourceFile with invalid characters and path separators fixed.</param>
		/// <param name="bundleToExclude">The Bundle to exclude from the search.</param>
		/// <returns>The resolved ResourceFile if it exists, else null.</returns>
		static ResourceFile? TryResolveFromChildBundles(Bundle currentBundle, string originalName, string fixedName, Bundle? bundleToExclude)
		{
			foreach (Bundle bundle in currentBundle.Bundles)
			{
				if (bundle != bundleToExclude && TryResolveFromResources(bundle, fixedName) is { } resource)
				{
					return resource;
				}
			}

			return null;
		}
	}

	protected virtual ResourceFile? ResolveExternalResource(string originalName) => null;

	/// <summary>
	/// Adds a ResourceFile to this Bundle.
	/// </summary>
	/// <param name="resource">The ResourceFile to add.</param>
	public void AddResource(ResourceFile resource)
	{
		resources.Add(resource);
	}

	/// <summary>
	/// Adds an <see cref="AssetCollection"/> to this Bundle.
	/// </summary>
	/// <param name="collection">The <see cref="AssetCollection"/> to add.</param>
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

	/// <summary>
	/// Adds a child Bundle to this Bundle.
	/// </summary>
	/// <param name="bundle">The Bundle to add.</param>
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

	public void AddFailed(FailedFile file)
	{
		failedFiles.Add(file);
	}

	/// <summary>
	/// Indicates if the specified <see cref="AssetCollection"/> is compatible with this Bundle.
	/// </summary>
	/// <param name="collection">The <see cref="AssetCollection"/> to check.</param>
	/// <returns>True if the <see cref="AssetCollection"/> is compatible, else false.</returns>
	protected virtual bool IsCompatibleCollection(AssetCollection collection) => true;

	/// <summary>
	/// Indicates if the specified Bundle is compatible with this Bundle.
	/// </summary>
	/// <param name="bundle">The Bundle to check.</param>
	/// <returns>True if the Bundle is compatible, else false.</returns>
	protected virtual bool IsCompatibleBundle(Bundle bundle) => bundle is not GameBundle;

	/// <summary>
	/// Gets the root Bundle of this Bundle.
	/// </summary>
	/// <returns>The root Bundle of this Bundle.</returns>
	public Bundle GetRoot()
	{
		Bundle root = this;
		while (root.Parent is not null)
		{
			root = root.Parent;
		}
		return root;
	}

	/// <summary>
	/// Fetches all <see cref="IUnityObjectBase"/>s in the hierarchy of this Bundle.
	/// </summary>
	/// <returns>An IEnumerable of all <see cref="IUnityObjectBase"/>s in the hierarchy.</returns>
	public IEnumerable<IUnityObjectBase> FetchAssetsInHierarchy()
	{
		return GetRoot().FetchAssets();
	}

	/// <summary>
	/// Fetches all <see cref="IUnityObjectBase"/>s in this Bundle.
	/// </summary>
	/// <returns>An IEnumerable of all <see cref="IUnityObjectBase"/>s in this Bundle.</returns>
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

	/// <summary>
	/// Fetches all AssetCollections in the hierarchy of this Bundle.
	/// </summary>
	/// <returns>An IEnumerable of all AssetCollections in the hierarchy.</returns>
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

	public override string ToString()
	{
		return Name;
	}

	public SerializedAssetCollection AddCollectionFromSerializedFile(SerializedFile file, AssetFactoryBase factory, UnityVersion defaultVersion = default)
	{
		return SerializedAssetCollection.FromSerializedFile(this, file, factory, defaultVersion);
	}

	#region IDisposable Support
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

	// This code added to correctly implement the disposable pattern.
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
