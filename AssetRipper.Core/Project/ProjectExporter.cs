using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
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
using System.Collections.Generic;

namespace AssetRipper.Core.Project
{
	public class ProjectExporter
	{
		public event Action? EventExportPreparationStarted;
		public event Action? EventExportPreparationFinished;
		public event Action? EventExportStarted;
		public event Action<int, int>? EventExportProgressUpdated;
		public event Action? EventExportFinished;

		/// <summary>
		/// Exact type to the exporters that handle that type
		/// </summary>
		private readonly Dictionary<Type, Stack<IAssetExporter>> typeMap = new Dictionary<Type, Stack<IAssetExporter>>();
		/// <summary>
		/// List of type-exporter-allow pairs<br/>
		/// Type: the asset type<br/>
		/// IAssetExporter: the exporter that can handle that asset type<br/>
		/// Bool: allow the exporter to apply on inherited asset types?
		/// </summary>
		private readonly List<(Type, IAssetExporter, bool)> registeredExporters = new List<(Type, IAssetExporter, bool)>();

		//Exporters
		protected DefaultYamlExporter DefaultExporter { get; } = new DefaultYamlExporter();
		protected SceneYamlExporter SceneExporter { get; } = new SceneYamlExporter();
		protected ManagerAssetExporter ManagerExporter { get; } = new ManagerAssetExporter();
		protected BuildSettingsExporter BuildSettingsExporter { get; } = new BuildSettingsExporter();
		protected ScriptableObjectExporter ScriptableExporter { get; } = new ScriptableObjectExporter();
		protected DummyAssetExporter DummyExporter { get; } = new DummyAssetExporter();

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

			OverrideExporter<Classes.UnknownObject>(new UnknownObjectExporter(), false);
			OverrideExporter<Classes.UnreadableObject>(new UnreadableObjectExporter(), false);
		}

		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <typeparam name="T">The c sharp type of this asset type. Any inherited types also get this exporter.</typeparam>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
		public void OverrideExporter<T>(IAssetExporter exporter, bool allowInheritance)
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

		protected IExportCollection CreateCollection(VirtualSerializedFile file, IUnityObjectBase asset)
		{
			Stack<IAssetExporter> exporters = GetExporterStack(asset);
			foreach (IAssetExporter exporter in exporters)
			{
				if (exporter.IsHandle(asset))
				{
					return exporter.CreateCollection(file, asset);
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
				if (type == baseType || (allowInheritance && type.IsAssignableTo(baseType)))
				{
					result.Push(exporter);
				}
			}
			return result;
		}

		protected void OverrideDummyExporter<T>(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			DummyExporter.SetUpClassType(classType, isEmptyCollection, isMetaType);
			OverrideExporter<T>(DummyExporter, true);
		}

		public void Export(GameCollection fileCollection, CoreConfiguration options) => Export(fileCollection, fileCollection.FetchSerializedFiles(), options);
		public void Export(GameCollection fileCollection, SerializedFile file, CoreConfiguration options) => Export(fileCollection, new SerializedFile[] { file }, options);
		public void Export(GameCollection fileCollection, IEnumerable<SerializedFile> files, CoreConfiguration options)
		{
			EventExportPreparationStarted?.Invoke();

			LayoutInfo exportLayout = new LayoutInfo(options.Version, options.Platform, options.Flags);
			VirtualSerializedFile virtualFile = new VirtualSerializedFile(exportLayout);
			List<IExportCollection> collections = new();

			// speed up fetching
			List<IUnityObjectBase> depList = new();
			HashSet<IUnityObjectBase> depSet = new();
			HashSet<IUnityObjectBase> queued = new();

			foreach (SerializedFile file in files)
			{
				foreach (IUnityObjectBase asset in file.FetchAssets())
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
					DependencyContext context = new DependencyContext(exportLayout, true);
					foreach (PPtr<IUnityObjectBase> pointer in dependent.FetchDependencies(context))
					{
						if (pointer.IsNull)
						{
							continue;
						}

						IUnityObjectBase? dependency = pointer.TryGetAsset(asset.SerializedFile);
						if (dependency == null)
						{
							string hierarchy = $"[{asset.SerializedFile.Name}]" + asset.SerializedFile.GetAssetLogString(asset.PathID) + "." + context.GetPointerPath();
							Logger.Log(LogType.Warning, LogCategory.Export, $"{hierarchy}'s dependency {context.PointerName} = {pointer.ToLogString(asset.SerializedFile)} wasn't found");
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
	}
}
