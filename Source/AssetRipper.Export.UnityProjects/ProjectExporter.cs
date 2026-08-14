using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Export.UnityProjects;

public sealed partial class ProjectExporter
{
	public event Action? EventExportPreparationStarted;
	public event Action? EventExportPreparationFinished;
	public event Action? EventExportStarted;
	public event Action<int, int>? EventExportProgressUpdated;
	public event Action? EventExportFinished;

	private readonly ObjectHandlerStack<IAssetExporter> assetExporterStack = new();

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
		WriteSanitizedNameMap(collections, options.ProjectRootPath, fileSystem);
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
				string displayName = ExportNameUtilities.GetSafeCollectionDisplayName(collection);
				Logger.Info(LogCategory.ExportProgress, $"({currentExportable}/{exportableCount}) Exporting '{displayName}'");
				bool exportedSuccessfully = collection.Export(container, options.ProjectRootPath, fileSystem);
				if (!exportedSuccessfully)
				{
					Logger.Warning(LogCategory.ExportProgress, $"Failed to export '{displayName}' ({collection.GetType().Name})");
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

		foreach (IUnityObjectBase asset in fileCollection.FetchAssets())
		{
			if (!queued.Contains(asset))
			{
				IExportCollection collection = CreateCollection(asset);
				foreach (IUnityObjectBase element in collection.Assets)
				{
					queued.Add(element);
				}
				collections.Add(collection);
			}
		}

		return collections;
	}

	private static void WriteSanitizedNameMap(IEnumerable<IExportCollection> collections, string projectDirectory, FileSystem fileSystem)
	{
		List<(IUnityObjectBase Asset, string OriginalName, string SafeName)> entries = [];
		HashSet<IUnityObjectBase> seenAssets = [];
		foreach (IUnityObjectBase asset in collections.SelectMany(static collection => collection.Assets))
		{
			if (!seenAssets.Add(asset))
			{
				continue;
			}

			string originalName = asset.GetBestName();
			string safeName = ExportNameUtilities.GetSafeAssetName(asset);
			if (!string.Equals(originalName, safeName, StringComparison.Ordinal))
			{
				entries.Add((asset, originalName, safeName));
			}
		}

		if (entries.Count == 0)
		{
			return;
		}

		fileSystem.Directory.Create(projectDirectory);
		string mapPath = fileSystem.Path.Join(projectDirectory, "AssetRipper_NameMap.tsv");
		using Stream stream = fileSystem.File.Create(mapPath);
		using InvariantStreamWriter writer = new(stream, new System.Text.UTF8Encoding(false));
		writer.WriteLine("Class\tPathID\tSafeName\tOriginalNameEscaped");
		foreach ((IUnityObjectBase asset, string originalName, string safeName) in entries
			.OrderBy(static entry => entry.Asset.ClassName, StringComparer.Ordinal)
			.ThenBy(static entry => entry.Asset.PathID))
		{
			writer.Write(asset.ClassName);
			writer.Write('\t');
			writer.Write(asset.PathID);
			writer.Write('\t');
			writer.Write(safeName);
			writer.Write('\t');
			writer.WriteLine(FileSystem.EscapeUnsafeUnicode(originalName));
		}

		Logger.Info(LogCategory.Export, $"Saved {entries.Count} sanitized asset names to AssetRipper_NameMap.tsv");
	}
}
