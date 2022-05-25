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
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_6;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Project
{
	public abstract class ProjectExporterBase
	{
		public event Action? EventExportPreparationStarted;
		public event Action? EventExportPreparationFinished;
		public event Action? EventExportStarted;
		public event Action<int, int>? EventExportProgressUpdated;
		public event Action? EventExportFinished;

		//Exporters
		protected DefaultYamlExporter DefaultExporter { get; } = new DefaultYamlExporter();
		protected SceneYamlExporter SceneExporter { get; } = new SceneYamlExporter();
		protected ManagerAssetExporter ManagerExporter { get; } = new ManagerAssetExporter();
		protected BuildSettingsExporter BuildSettingsExporter { get; } = new BuildSettingsExporter();
		protected ScriptableObjectExporter ScriptableExporter { get; } = new ScriptableObjectExporter();
		protected DummyAssetExporter DummyExporter { get; } = new DummyAssetExporter();

		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <typeparam name="T">The c sharp type of this asset type. Any inherited types also get this exporter.</typeparam>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
		public void OverrideExporter<T>(IAssetExporter exporter, bool allowInheritance) => OverrideExporter(typeof(T), exporter, allowInheritance);
		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <param name="type">The c sharp type of this asset type. Any inherited types also get this exporter.</param>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
		public abstract void OverrideExporter(Type type, IAssetExporter exporter, bool allowInheritance);

		public abstract AssetType ToExportType(Type type);
		protected abstract IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset);

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
			List<IExportCollection> collections = new List<IExportCollection>();

			foreach (SerializedFile file in files)
			{
				foreach (IUnityObjectBase asset in file.FetchAssets())
				{
					asset.ConvertToEditor();
					//StaticMeshConverter.MaybeReplaceStaticMesh(asset, file, virtualFile);
				}
			}

			// speed up fetching
			List<IUnityObjectBase> depList = new List<IUnityObjectBase>();
			HashSet<IUnityObjectBase> depSet = new HashSet<IUnityObjectBase>();
			HashSet<IUnityObjectBase> queued = new HashSet<IUnityObjectBase>();

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

						IUnityObjectBase? dependency = pointer.FindAsset(asset.SerializedFile);
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

		public ProjectExporterBase()
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
			OverrideDummyExporter<IMonoManager>(ClassIDType.MonoManager, true, false);
			OverrideDummyExporter<IAssetBundle>(ClassIDType.AssetBundle, true, false);
			OverrideDummyExporter<IResourceManager>(ClassIDType.ResourceManager, true, false);
		}
	}
}
