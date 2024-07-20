using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.Export.PrimaryContent.Audio;
using AssetRipper.Export.PrimaryContent.DeletedAssets;
using AssetRipper.Export.PrimaryContent.Models;
using AssetRipper.Export.PrimaryContent.Scripts;
using AssetRipper.Export.PrimaryContent.Textures;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_152;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_238;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_329;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_83;

namespace AssetRipper.Export.PrimaryContent;

public sealed class PrimaryContentExporter
{
	private readonly ObjectHandlerStack<IContentExtractor> exporters = new();
	private readonly GameData gameData;

	private PrimaryContentExporter(GameData gameData)
	{
		this.gameData = gameData;
	}

	public void RegisterHandler<T>(IContentExtractor handler, bool allowInheritance = true) where T : IUnityObjectBase
	{
		exporters.OverrideHandler(typeof(T), handler, allowInheritance);
	}

	public void RegisterHandler(Type type, IContentExtractor handler, bool allowInheritance = true)
	{
		exporters.OverrideHandler(type, handler, allowInheritance);
	}

	public static PrimaryContentExporter CreateDefault(GameData gameData)
	{
		PrimaryContentExporter exporter = new(gameData);
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

		RegisterHandler<IMesh>(new GlbMeshExporter());

		RegisterHandler<INavMeshData>(new GlbNavMeshExporter());
		RegisterHandler<ITerrainData>(new GlbTerrainExporter());

		RegisterHandler<ITextAsset>(BinaryAssetContentExtractor.Instance);
		RegisterHandler<IFont>(BinaryAssetContentExtractor.Instance);
		RegisterHandler<IMovieTexture>(BinaryAssetContentExtractor.Instance);
		RegisterHandler<IVideoClip>(BinaryAssetContentExtractor.Instance);

		RegisterHandler<IAudioClip>(new AudioContentExtractor());

		RegisterHandler<IImageTexture>(new TextureExporter(ImageExportFormat.Png));

		RegisterHandler<IMonoScript>(new ScriptContentExtractor(gameData.AssemblyManager));

		// Deleted assets
		// This must be the last handler
		RegisterHandler<IUnityObjectBase>(DeletedAssetsExporter.Instance);
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
			if (queued.Add(asset))
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
