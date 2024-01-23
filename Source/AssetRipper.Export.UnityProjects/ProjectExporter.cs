using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated;

namespace AssetRipper.Export.UnityProjects
{
	public sealed partial class ProjectExporter
	{
		public event Action? EventExportPreparationStarted;
		public event Action? EventExportPreparationFinished;
		public event Action? EventExportStarted;
		public event Action<int, int>? EventExportProgressUpdated;
		public event Action? EventExportFinished;

		/// <summary>
		/// Exact type to the exporters that handle that type
		/// </summary>
		private readonly Dictionary<Type, Stack<IAssetExporter>> typeMap = new();
		/// <summary>
		/// List of type-exporter-allow pairs<br/>
		/// Type: the asset type<br/>
		/// IAssetExporter: the exporter that can handle that asset type<br/>
		/// Bool: allow the exporter to apply on inherited asset types?
		/// </summary>
		private readonly List<(Type, IAssetExporter, bool)> registeredExporters = new();

		//Exporters
		private DummyAssetExporter DummyExporter { get; } = new DummyAssetExporter();

		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <typeparam name="T">The c sharp type of this asset type. Any inherited types also get this exporter.</typeparam>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
		public void OverrideExporter<T>(IAssetExporter exporter, bool allowInheritance = true)
		{
			OverrideExporter(typeof(T), exporter, allowInheritance);
		}

		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <param name="type">The c sharp type of this asset type. Any inherited types also get this exporter.</param>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
		public void OverrideExporter(Type type, IAssetExporter exporter, bool allowInheritance)
		{
			ArgumentNullException.ThrowIfNull(exporter);

			registeredExporters.Add((type, exporter, allowInheritance));
			if (typeMap.Count > 0)//Just in case an exporter gets added after CreateCollection or ToExportType have already been used
			{
				RecalculateTypeMap();
			}
		}

		/// <summary>
		/// Use the <see cref="DummyExporter"/> for the specified class type.
		/// </summary>
		/// <typeparam name="T">The base type for assets of that <paramref name="classType"/>.</typeparam>
		/// <param name="classType">The class id of assets we are using the <see cref="DummyExporter"/> for.</param>
		/// <param name="isEmptyCollection">
		/// True: an exception will be thrown if the asset is referenced by another asset.<br/>
		/// False: any references to this asset will be replaced with a missing reference.
		/// </param>
		/// <param name="isMetaType"><see cref="AssetType.Meta"/> or <see cref="AssetType.Serialized"/>?</param>
		private void OverrideDummyExporter<T>(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			DummyExporter.SetUpClassType(classType, isEmptyCollection, isMetaType);
			OverrideExporter<T>(DummyExporter, true);
		}

		public AssetType ToExportType(Type type)
		{
			Stack<IAssetExporter> exporters = GetExporterStack(type);
			foreach (IAssetExporter exporter in exporters)
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
			foreach (IAssetExporter exporter in GetExporterStack(asset))
			{
				if (exporter.TryCreateCollection(asset, out IExportCollection? collection))
				{
					return collection;
				}
			}
			throw new Exception($"There is no exporter that can handle '{asset}'");
		}

		private Stack<IAssetExporter> GetExporterStack(IUnityObjectBase asset) => GetExporterStack(asset.GetType());
		private Stack<IAssetExporter> GetExporterStack(Type type)
		{
			if (!typeMap.TryGetValue(type, out Stack<IAssetExporter>? exporters))
			{
				exporters = CalculateAssetExporterStack(type);
				typeMap.Add(type, exporters);
			}
			return exporters;
		}

		private void RecalculateTypeMap()
		{
			foreach (Type type in typeMap.Keys)
			{
				typeMap[type] = CalculateAssetExporterStack(type);
			}
		}

		private Stack<IAssetExporter> CalculateAssetExporterStack(Type type)
		{
			Stack<IAssetExporter> result = new();
			foreach ((Type baseType, IAssetExporter exporter, bool allowInheritance) in registeredExporters)
			{
				if (type == baseType || allowInheritance && type.IsAssignableTo(baseType))
				{
					result.Push(exporter);
				}
			}
			return result;
		}

		public void Export(GameBundle fileCollection, CoreConfiguration options)
		{
			EventExportPreparationStarted?.Invoke();
			List<IExportCollection> collections = CreateCollections(fileCollection);
			EventExportPreparationFinished?.Invoke();

			EventExportStarted?.Invoke();
			ProjectAssetContainer container = new ProjectAssetContainer(this, options, fileCollection.FetchAssets(), collections);
			for (int i = 0; i < collections.Count; i++)
			{
				IExportCollection collection = collections[i];
				container.CurrentCollection = collection;
				if (collection.Exportable)
				{
					Logger.Info(LogCategory.ExportProgress, $"Exporting '{collection.Name}'");
					bool exportedSuccessfully = collection.Export(container, options.ProjectRootPath);
					if (!exportedSuccessfully)
					{
						Logger.Warning(LogCategory.ExportProgress, $"Failed to export '{collection.Name}'");
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
	}
}
