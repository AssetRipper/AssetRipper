using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Core.Project
{
	public abstract class ProjectExporterBase : IProjectExporter
	{
		public event Action EventExportPreparationStarted;
		public event Action EventExportPreparationFinished;
		public event Action EventExportStarted;
		public event Action<int, int> EventExportProgressUpdated;
		public event Action EventExportFinished;

		public void Export(GameCollection fileCollection, CoreConfiguration options) => Export(fileCollection, fileCollection.FetchSerializedFiles(), options);
		public void Export(GameCollection fileCollection, SerializedFile file, CoreConfiguration options) => Export(fileCollection, new SerializedFile[] { file }, options);
		public void Export(GameCollection fileCollection, IEnumerable<SerializedFile> files, CoreConfiguration options)
		{
			EventExportPreparationStarted?.Invoke();

			LayoutInfo info = new LayoutInfo(options.Version, options.Platform, options.Flags);
			AssetLayout exportLayout = new AssetLayout(info);
			VirtualSerializedFile virtualFile = new VirtualSerializedFile(exportLayout);
			List<IExportCollection> collections = new List<IExportCollection>();

			// speed up fetching
			List<UnityObjectBase> depList = new List<UnityObjectBase>();
			HashSet<UnityObjectBase> depSet = new HashSet<UnityObjectBase>();
			HashSet<UnityObjectBase> queued = new HashSet<UnityObjectBase>();

			foreach (SerializedFile file in files)
			{
				foreach (UnityObjectBase asset in file.FetchAssets())
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
				UnityObjectBase asset = depList[i];
				if (!queued.Contains(asset))
				{
					IExportCollection collection = CreateCollection(virtualFile, asset);
					foreach (UnityObjectBase element in collection.Assets)
					{
						queued.Add(element);
					}
					collections.Add(collection);
				}

				if (options.ExportDependencies && asset is IDependent dependent)
				{
					DependencyContext context = new DependencyContext(exportLayout, true);
					foreach (PPtr<UnityObjectBase> pointer in dependent.FetchDependencies(context))
					{
						if (pointer.IsNull)
						{
							continue;
						}

						UnityObjectBase dependency = pointer.FindAsset(asset.File);
						if (dependency == null)
						{
							string hierarchy = $"[{asset.File.Name}]" + asset.File.GetAssetLogString(asset.PathID) + "." + context.GetPointerPath();
							Logger.Log(LogType.Warning, LogCategory.Export, $"{hierarchy}'s dependency {context.PointerName} = {pointer.ToLogString(asset.File)} wasn't found");
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
				bool isExported = collection.Export(container, options.ExportPath);
				if (isExported)
				{
					Logger.Info(LogCategory.ExportedFile, $"'{collection.Name}' exported");
				}
				EventExportProgressUpdated?.Invoke(i, collections.Count);
			}
			EventExportFinished?.Invoke();
		}

		public abstract void OverrideExporter(ClassIDType classType, IAssetExporter exporter);
		public abstract AssetType ToExportType(ClassIDType classID);
		protected abstract IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset);
	}
}
