using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Export.PrimaryContent.Models;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_238;
using AssetRipper.SourceGenerated.Classes.ClassID_3;

namespace AssetRipper.Export.PrimaryContent;

public sealed class PrimaryContentExporter
{
	private readonly ObjectHandlerStack<IContentExtractor> exporters = new();

	public void RegisterHandler<T>(IContentExtractor handler, bool allowInheritance = true) where T : IUnityObjectBase
	{
		exporters.OverrideHandler(typeof(T), handler, allowInheritance);
	}

	public void RegisterHandler(Type type, IContentExtractor handler, bool allowInheritance = true)
	{
		exporters.OverrideHandler(type, handler, allowInheritance);
	}

	public static PrimaryContentExporter CreateDefault()
	{
		PrimaryContentExporter exporter = new();
		exporter.RegisterDefaultHandlers();
		return exporter;
	}

	private void RegisterDefaultHandlers()
	{
		RegisterHandler<IUnityObjectBase>(new JsonContentExtractor());

		GlbModelExporter modelExporter = new();
		RegisterHandler<GameObjectHierarchyObject>(modelExporter);
		RegisterHandler<IGameObject>(modelExporter);
		RegisterHandler<IComponent>(modelExporter);
		RegisterHandler<ILevelGameManager>(modelExporter);

		RegisterHandler<INavMeshData>(new GlbNavMeshExporter());
		RegisterHandler<ITerrainData>(new GlbTerrainExporter());

	}

	public void Export(GameBundle fileCollection, CoreConfiguration options)
	{
		List<ExportCollectionBase> collections = CreateCollections(fileCollection);

		foreach (ExportCollectionBase collection in collections)
		{
			if (collection.Exportable)
			{
				Logger.Info(LogCategory.ExportProgress, $"Exporting '{collection.Name}'");
				bool exportedSuccessfully = collection.Export(options.ExportRootPath);
				if (!exportedSuccessfully)
				{
					Logger.Warning(LogCategory.ExportProgress, $"Failed to export '{collection.Name}'");
				}
			}
		}
	}

	private List<ExportCollectionBase> CreateCollections(GameBundle fileCollection)
	{
		List<ExportCollectionBase> collections = new();
		HashSet<IUnityObjectBase> queued = new();

		foreach (IUnityObjectBase asset in fileCollection.FetchAssets())
		{
			if (!queued.Contains(asset))
			{
				ExportCollectionBase collection = CreateCollection(asset);
				foreach (IUnityObjectBase element in collection.Assets)
				{
					queued.Add(element);
				}
				collections.Add(collection);
			}
		}

		return collections;
	}

	private ExportCollectionBase CreateCollection(IUnityObjectBase asset)
	{
		foreach (IContentExtractor exporter in exporters.GetHandlerStack(asset.GetType()))
		{
			if (exporter.TryCreateCollection(asset, out ExportCollectionBase? collection))
			{
				return collection;
			}
		}
		throw new Exception($"There is no exporter that can handle '{asset}'");
	}
}
