using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.PrimaryContent.Audio;
using AssetRipper.Export.PrimaryContent.DeletedAssets;
using AssetRipper.Export.PrimaryContent.Models;
using AssetRipper.Export.PrimaryContent.Scripts;
using AssetRipper.Export.PrimaryContent.Textures;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using AssetRipper.Processing.Prefabs;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;
using AssetRipper.SourceGenerated.Classes.ClassID_1101;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_1109;
using AssetRipper.SourceGenerated.Classes.ClassID_111;
using AssetRipper.SourceGenerated.Classes.ClassID_1111;
using AssetRipper.SourceGenerated.Classes.ClassID_1120;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_150;
using AssetRipper.SourceGenerated.Classes.ClassID_152;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_206;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_221;
using AssetRipper.SourceGenerated.Classes.ClassID_238;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_329;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_72;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Classes.ClassID_90;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Classes.ClassID_93;
using AssetRipper.SourceGenerated.Classes.ClassID_95;

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

		RegisterEmptyHandler<IAnimation>();
		RegisterEmptyHandler<IAnimationClip>();
		RegisterEmptyHandler<IAnimator>();
		RegisterEmptyHandler<IAnimatorController>();
		RegisterEmptyHandler<IAnimatorOverrideController>();
		RegisterEmptyHandler<IAnimatorState>();
		RegisterEmptyHandler<IAnimatorStateMachine>();
		RegisterEmptyHandler<IAnimatorStateTransition>();
		RegisterEmptyHandler<IAnimatorTransition>();
		RegisterEmptyHandler<IAnimatorTransitionBase>();
		RegisterEmptyHandler<IAvatar>();
		RegisterEmptyHandler<IBlendTree>();
		RegisterEmptyHandler<IComponent>();
		RegisterEmptyHandler<IComputeShader>();
		RegisterEmptyHandler<ILightingDataAsset>();
		RegisterEmptyHandler<IMaterial>();
		RegisterEmptyHandler<IPreloadData>();
		RegisterEmptyHandler<IRuntimeAnimatorController>();
		RegisterEmptyHandler<ISceneAsset>();
		RegisterEmptyHandler<SpriteInformationObject>();

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

	public void Export(GameBundle fileCollection, CoreConfiguration options, FileSystem fileSystem)
	{
		List<ExportCollectionBase> collections = CreateCollections(fileCollection);

		for (int i = 0; i < collections.Count; i++)
		{
			ExportCollectionBase collection = collections[i];
			if (collection.Exportable)
			{
				Logger.Info(LogCategory.ExportProgress, $"({i + 1}/{collections.Count}) Exporting '{collection.Name}'");
				bool exportedSuccessfully = collection.Export(options.ExportRootPath, fileSystem);
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
			if (!queued.Add(asset))
			{
				// Skip duplicates
				continue;
			}

			ExportCollectionBase collection = CreateCollection(asset);
			if (collection is EmptyExportCollection)
			{
				// Skip empty collections. The asset has already been added to the hash set.
				continue;
			}

			foreach (IUnityObjectBase element in collection.Assets)
			{
				queued.Add(element);
			}
			collections.Add(collection);
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

	private void RegisterEmptyHandler<T>() where T : IUnityObjectBase
	{
		RegisterHandler<T>(EmptyContentExtractor.Instance);
	}
}
