using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Bundles;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_72;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Classes.ClassID_213;

namespace AssetRipper.Export.UnityProjects;

public sealed partial class ProjectExporter
{
	public event Action? EventExportPreparationStarted;
	public event Action? EventExportPreparationFinished;
	public event Action? EventExportStarted;
	public event Action<int, int>? EventExportProgressUpdated;
	public event Action? EventExportFinished;

	private readonly ObjectHandlerStack<IAssetExporter> assetExporterStack = new();
	private readonly bool enableAssetDeduplication;

	/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
	/// <typeparam name="T">The c sharp type of this asset type. Any inherited types also get this exporter.</typeparam>
	/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
	/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
	public void OverrideExporter<T>(IAssetExporter exporter, bool allowInheritance = true)
	{
		assetExporterStack.OverrideHandler(typeof(T), exporter, allowInheritance);
	}

	/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
	/// <param name="type">The c sharp type of this asset type. Any inherited types also get this exporter.</param>
	/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
	/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
	public void OverrideExporter(Type type, IAssetExporter exporter, bool allowInheritance)
	{
		assetExporterStack.OverrideHandler(type, exporter, allowInheritance);
	}

	/// <summary>
	/// Use the <see cref="DummyExporter"/> for the specified class type.
	/// </summary>
	/// <typeparam name="T">The base type for assets of that <paramref name="classType"/>.</typeparam>
	/// <param name="isEmptyCollection">
	/// True: an exception will be thrown if the asset is referenced by another asset.<br/>
	/// False: any references to this asset will be replaced with a missing reference.
	/// </param>
	/// <param name="isMetaType"><see cref="AssetType.Meta"/> or <see cref="AssetType.Serialized"/>?</param>
	private void OverrideDummyExporter<T>(bool isEmptyCollection, bool isMetaType)
	{
		OverrideExporter<T>(DummyAssetExporter.Get(isEmptyCollection, isMetaType), true);
	}

	public AssetType ToExportType(Type type)
	{
		foreach (IAssetExporter exporter in assetExporterStack.GetHandlerStack(type))
		{
			if (exporter.ToUnknownExportType(type, out AssetType assetType))
			{
				return assetType;
			}
		}
		throw new NotSupportedException($"There is no exporter that know {nameof(AssetType)} for unknown asset '{type}'");
	}

	private IExportCollection CreateCollection(IUnityObjectBase asset)
	{
		foreach (IAssetExporter exporter in assetExporterStack.GetHandlerStack(asset.GetType()))
		{
			if (exporter.TryCreateCollection(asset, out IExportCollection? collection))
			{
				return collection;
			}
		}
		throw new Exception($"There is no exporter that can handle '{asset}'");
	}

	public void Export(GameBundle fileCollection, CoreConfiguration options, FileSystem fileSystem)
	{
		EventExportPreparationStarted?.Invoke();
		List<IExportCollection> collections = CreateCollections(fileCollection);
		EventExportPreparationFinished?.Invoke();

		EventExportStarted?.Invoke();
		ProjectAssetContainer container = new ProjectAssetContainer(this, options, fileCollection.FetchAssets(), collections);
		int exportableCount = collections.Count(c => c.Exportable);
		int currentExportable = 0;

		for (int i = 0; i < collections.Count; i++)
		{
			IExportCollection collection = collections[i];
			container.CurrentCollection = collection;
			if (collection.Exportable)
			{
				currentExportable++;
				Logger.Info(LogCategory.ExportProgress, $"({currentExportable}/{exportableCount}) Exporting '{collection.Name}'");
				bool exportedSuccessfully;
				bool failureAlreadyLogged = false;
				try
				{
					exportedSuccessfully = collection.Export(container, options.ProjectRootPath, fileSystem);
				}
				catch (UnauthorizedAccessException ex)
				{
					exportedSuccessfully = false;
					failureAlreadyLogged = true;
					Logger.Warning(LogCategory.ExportProgress, $"Failed to export '{collection.Name}' ({collection.GetType().Name}) due filesystem access denial.");
					Logger.Warning(LogCategory.ExportProgress, ex.Message);
					Logger.Verbose(LogCategory.ExportProgress, ex.ToString());
				}
				catch (Exception ex)
				{
					exportedSuccessfully = false;
					failureAlreadyLogged = true;
					Logger.Warning(LogCategory.ExportProgress, $"Failed to export '{collection.Name}' ({collection.GetType().Name}) due exception: {ex.GetType().Name}: {ex.Message}");
					Logger.Verbose(LogCategory.ExportProgress, ex.ToString());
				}
				if (!exportedSuccessfully && !failureAlreadyLogged)
				{
					Logger.Warning(LogCategory.ExportProgress, $"Failed to export '{collection.Name}' ({collection.GetType().Name})");
				}
			}
			EventExportProgressUpdated?.Invoke(i, collections.Count);
		}
		EventExportFinished?.Invoke();
	}

	private List<IExportCollection> CreateCollections(GameBundle fileCollection)
	{
		List<IExportCollection> collections = new();
		HashSet<IUnityObjectBase> queued = new();
		AssetEqualityComparer? equalityComparer = enableAssetDeduplication ? new AssetEqualityComparer() : null;
		Dictionary<Type, Dictionary<int, List<IUnityObjectBase>>> dedupBuckets = new();
		int redirectedAssetCount = 0;

		foreach (IUnityObjectBase asset in fileCollection.FetchAssets())
		{
			if (!queued.Contains(asset))
			{
				IExportCollection collection;
				if (TryCreateDeduplicatedCollection(asset, equalityComparer, dedupBuckets, out IExportCollection? deduplicatedCollection))
				{
					collection = deduplicatedCollection;
					redirectedAssetCount++;
				}
				else
				{
					collection = CreateCollection(asset);
					if (collection.Exportable)
					{
						RegisterDedupCandidates(collection.Assets, dedupBuckets);
					}
				}
				foreach (IUnityObjectBase element in collection.Assets)
				{
					queued.Add(element);
				}
				collections.Add(collection);
			}
		}

		if (redirectedAssetCount > 0)
		{
			Logger.Info(LogCategory.Export, $"Asset deduplication redirected {redirectedAssetCount} assets.");
		}

		return collections;
	}

	private bool TryCreateDeduplicatedCollection(
		IUnityObjectBase asset,
		AssetEqualityComparer? equalityComparer,
		Dictionary<Type, Dictionary<int, List<IUnityObjectBase>>> dedupBuckets,
		[NotNullWhen(true)] out IExportCollection? collection)
	{
		if (!enableAssetDeduplication || equalityComparer is null || !CanDeduplicateAsset(asset))
		{
			collection = null;
			return false;
		}

		if (!TryGetDedupBucket(asset, dedupBuckets, false, out List<IUnityObjectBase>? candidates))
		{
			collection = null;
			return false;
		}

		foreach (IUnityObjectBase candidate in candidates)
		{
			try
			{
				if (equalityComparer.Equals(candidate, asset))
				{
					collection = new AssetRedirectExportCollection(asset, candidate);
					return true;
				}
			}
			catch (Exception ex)
			{
				Logger.Verbose(LogCategory.Export, $"Skipping asset deduplication comparison for '{asset.GetBestName()}' because {ex.GetType().Name} was thrown.");
			}
		}

		collection = null;
		return false;
	}

	private void RegisterDedupCandidates(IEnumerable<IUnityObjectBase> assets, Dictionary<Type, Dictionary<int, List<IUnityObjectBase>>> dedupBuckets)
	{
		if (!enableAssetDeduplication)
		{
			return;
		}

		foreach (IUnityObjectBase asset in assets)
		{
			if (!CanDeduplicateAsset(asset))
			{
				continue;
			}

			TryGetDedupBucket(asset, dedupBuckets, true, out List<IUnityObjectBase>? candidates);
			candidates!.Add(asset);
		}
	}

	private static bool TryGetDedupBucket(
		IUnityObjectBase asset,
		Dictionary<Type, Dictionary<int, List<IUnityObjectBase>>> dedupBuckets,
		bool create,
		[NotNullWhen(true)] out List<IUnityObjectBase>? bucket)
	{
		Type assetType = asset.GetType();
		if (!dedupBuckets.TryGetValue(assetType, out Dictionary<int, List<IUnityObjectBase>>? typeBuckets))
		{
			if (!create)
			{
				bucket = null;
				return false;
			}

			typeBuckets = new Dictionary<int, List<IUnityObjectBase>>();
			dedupBuckets.Add(assetType, typeBuckets);
		}

		int hash = HashCode.Combine(assetType, asset.GetBestName());
		if (!typeBuckets.TryGetValue(hash, out bucket))
		{
			if (!create)
			{
				return false;
			}

			bucket = new List<IUnityObjectBase>();
			typeBuckets.Add(hash, bucket);
		}

		return true;
	}

	private static bool CanDeduplicateAsset(IUnityObjectBase asset)
	{
		return asset switch
		{
			IMonoScript => false,
			IShader => true,
			IComputeShader => true,
			IAudioClip => true,
			ITextAsset => true,
			IMesh => true,
			IImageTexture texture when texture.MainAsset is null && texture is not ISprite => true,
			_ => false,
		};
	}
}
