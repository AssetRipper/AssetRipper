using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Export.UnityProjects.RawAssets;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
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

namespace AssetRipper.Export.UnityProjects
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
		private DummyAssetExporter DummyExporter { get; } = new DummyAssetExporter();

		public ProjectExporter()
		{
			OverrideExporter<IUnityObjectBase>(new DefaultYamlExporter(), true);

			OverrideExporter<IGlobalGameManager>(new ManagerAssetExporter(), true);

			OverrideExporter<IBuildSettings>(new BuildSettingsExporter(), true);

			OverrideExporter<IMonoBehaviour>(new ScriptableObjectExporter(), true);

			SceneYamlExporter sceneExporter = new();
			OverrideExporter<IPrefabInstance>(sceneExporter, true);
			OverrideExporter<IGameObject>(sceneExporter, true);
			OverrideExporter<IComponent>(sceneExporter, true);
			OverrideExporter<ILevelGameManager>(sceneExporter, true);

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
			foreach (IAssetExporter exporter in GetExporterStack(asset))
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

				if (options.ExportDependencies)
				{
					foreach ((string path, PPtr pointer) in asset.FetchDependencies())
					{
						if (pointer.IsNull)
						{
							continue;
						}

						IUnityObjectBase? dependency = asset.Collection.TryGetAsset(pointer);
						if (dependency == null)
						{
							string hierarchy = $"[{asset.Collection.Name}]" + asset.Collection.GetAssetLogString(asset.PathID) + "." + path;
							Logger.Log(LogType.Warning, LogCategory.Export, $"{hierarchy}'s dependency = {ToLogString(pointer, asset.Collection)} wasn't found");
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
			ProjectAssetContainer container = new ProjectAssetContainer(this, virtualFile, fileCollection.FetchAssets(), collections);
			for (int i = 0; i < collections.Count; i++)
			{
				IExportCollection collection = collections[i];
				container.CurrentCollection = collection;
				if (collection is not EmptyExportCollection)
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

		private static string ToLogString(PPtr pptr, IAssetContainer container)
		{
			string depName = pptr.FileID == 0 ? container.Name : container.Dependencies[pptr.FileID - 1]?.Name ?? "Not Found";
			return $"[{depName}]_{pptr.PathID}";
		}
	}
}
