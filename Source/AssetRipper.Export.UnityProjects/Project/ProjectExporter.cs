using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.Import.Classes;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_116;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_147;
using AssetRipper.SourceGenerated.Classes.ClassID_150;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_290;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_6;
using AssetRipper.SourceGenerated.Classes.ClassID_94;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Project
{
	public sealed class ProjectExporter
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
		private DefaultYamlExporter DefaultExporter { get; } = new DefaultYamlExporter();
		private SceneYamlExporter SceneExporter { get; } = new SceneYamlExporter();
		private ManagerAssetExporter ManagerExporter { get; } = new ManagerAssetExporter();
		private BuildSettingsExporter BuildSettingsExporter { get; } = new BuildSettingsExporter();
		private ScriptableObjectExporter ScriptableExporter { get; } = new ScriptableObjectExporter();
		private DummyAssetExporter DummyExporter { get; } = new DummyAssetExporter();

		public ProjectExporter()
		{
			OverrideExporter<IUnityObjectBase>(new RawAssetExporter(), true);
			OverrideExporter<IUnityObjectBase>(DefaultExporter, true);

			OverrideExporter<IGlobalGameManager>(ManagerExporter, true);

			OverrideExporter<IBuildSettings>(BuildSettingsExporter, true);

			OverrideExporter<IMonoBehaviour>(ScriptableExporter, true);

			OverrideExporter<IGameObject>(SceneExporter, true);
			OverrideExporter<IComponent>(SceneExporter, true);
			OverrideExporter<ILevelGameManager>(SceneExporter, true);

			OverrideDummyExporter<IPreloadData>(ClassIDType.PreloadData, true, false);
			OverrideDummyExporter<IAssetBundle>(ClassIDType.AssetBundle, true, false);
			OverrideDummyExporter<IAssetBundleManifest>(ClassIDType.AssetBundleManifest, true, false);
			OverrideDummyExporter<IMonoManager>(ClassIDType.MonoManager, true, false);
			OverrideDummyExporter<IResourceManager>(ClassIDType.ResourceManager, true, false);
			OverrideDummyExporter<IShaderNameRegistry>(ClassIDType.ShaderNameRegistry, true, false);

			OverrideExporter<ISceneAsset>(new SceneAssetExporter(), true);
			OverrideExporter<UnknownObject>(new UnknownObjectExporter(), false);
			OverrideExporter<UnreadableObject>(new UnreadableObjectExporter(), false);
		}

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
			if (exporter == null)
			{
				throw new ArgumentNullException(nameof(exporter));
			}

			registeredExporters.Add((type, exporter, allowInheritance));
			if (typeMap.Count > 0)//Just in case an exporter gets added after CreateCollection or ToExportType have already been used
			{
				RecalculateTypeMap();
			}
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

		private IExportCollection CreateCollection(TemporaryAssetCollection file, IUnityObjectBase asset)
		{
			Stack<IAssetExporter> exporters = GetExporterStack(asset);
			foreach (IAssetExporter exporter in exporters)
			{
				if (exporter.TryCreateCollection(asset, file, out IExportCollection? collection))
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
			Stack<IAssetExporter> result = new Stack<IAssetExporter>();
			foreach ((Type baseType, IAssetExporter exporter, bool allowInheritance) in registeredExporters)
			{
				if (type == baseType || allowInheritance && type.IsAssignableTo(baseType))
				{
					result.Push(exporter);
				}
			}
			return result;
		}

		private void OverrideDummyExporter<T>(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			DummyExporter.SetUpClassType(classType, isEmptyCollection, isMetaType);
			OverrideExporter<T>(DummyExporter, true);
		}

		public void Export(GameBundle fileCollection, CoreConfiguration options) => Export(fileCollection, fileCollection.FetchAssetCollections(), options);
		public void Export(GameBundle fileCollection, AssetCollection file, CoreConfiguration options) => Export(fileCollection, new AssetCollection[] { file }, options);
		public void Export(GameBundle fileCollection, IEnumerable<AssetCollection> files, CoreConfiguration options)
		{
			EventExportPreparationStarted?.Invoke();

			fileCollection.ClearTemporaryBundles();
			TemporaryAssetCollection virtualFile = fileCollection.AddNewTemporaryBundle().AddNew();
			virtualFile.SetLayout(options.Version, options.Platform, options.Flags);

			List<IExportCollection> collections = new();

			// speed up fetching
			List<IUnityObjectBase> depList = new();
			HashSet<IUnityObjectBase> depSet = new();
			HashSet<IUnityObjectBase> queued = new();

			foreach (AssetCollection file in files)
			{
				foreach (IUnityObjectBase asset in file)
				{
					if (!options.Filter(asset))
					{
						continue;
					}

					depList.Add(asset);
					depSet.Add(asset);
				}
			}


			for (int i = 0; i < depList.Count; i++)
			{
				IUnityObjectBase asset = depList[i];
				if (!queued.Contains(asset))
				{
					IExportCollection collection = CreateCollection(virtualFile, asset);
					foreach (IUnityObjectBase element in collection.Assets)
					{
						queued.Add(element);
					}
					collections.Add(collection);
				}

				if (options.ExportDependencies && asset is IDependent dependent)
				{
					DependencyContext context = new DependencyContext(true);
					foreach (PPtr<IUnityObjectBase> pointer in dependent.FetchDependencies(context))
					{
						if (pointer.IsNull)
						{
							continue;
						}

						IUnityObjectBase? dependency = asset.Collection.TryGetAsset(pointer);
						if (dependency == null)
						{
							string hierarchy = $"[{asset.Collection.Name}]" + asset.Collection.GetAssetLogString(asset.PathID) + "." + context.GetPointerPath();
							Logger.Log(LogType.Warning, LogCategory.Export, $"{hierarchy}'s dependency {context.PointerName} = {ToLogString(pointer, asset.Collection)} wasn't found");
							continue;
						}

						if (!depSet.Contains(dependency))
						{
							depList.Add(dependency);
							depSet.Add(dependency);
						}
					}
				}
			}
			depList.Clear();
			depSet.Clear();
			queued.Clear();
			EventExportPreparationFinished?.Invoke();

			EventExportStarted?.Invoke();
			ProjectAssetContainer container = new ProjectAssetContainer(this, options, virtualFile, fileCollection.FetchAssets(), collections);
			for (int i = 0; i < collections.Count; i++)
			{
				IExportCollection collection = collections[i];
				container.CurrentCollection = collection;
				bool isExported = collection.Export(container, options.ProjectRootPath);
				if (isExported)
				{
					Logger.Info(LogCategory.ExportedFile, $"'{collection.Name}' exported");
				}
				else if (collection is not EmptyExportCollection)
				{
					Logger.Warning(LogCategory.ExportedFile, $"'{collection.Name}' failed to export");
				}
				EventExportProgressUpdated?.Invoke(i, collections.Count);
			}
			EventExportFinished?.Invoke();
		}

		private static string ToLogString<T>(PPtr<T> pptr, IAssetContainer container) where T : IUnityObjectBase
		{
			string depName = pptr.FileID == 0 ? container.Name : container.Dependencies[pptr.FileID - 1]?.Name ?? "Not Found";
			return $"[{depName}]{typeof(T).Name}_{pptr.PathID}";
		}
	}
}
