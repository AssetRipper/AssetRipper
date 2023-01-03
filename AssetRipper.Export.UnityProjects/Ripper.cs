using AssetRipper.Assets;
using AssetRipper.Core;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters;
using AssetRipper.Library.Exporters.AnimatorControllers;
using AssetRipper.Library.Exporters.Audio;
using AssetRipper.Library.Exporters.AudioMixers;
using AssetRipper.Library.Exporters.Meshes;
using AssetRipper.Library.Exporters.Miscellaneous;
using AssetRipper.Library.Exporters.Models;
using AssetRipper.Library.Exporters.NavMeshes;
using AssetRipper.Library.Exporters.PathIdMapping;
using AssetRipper.Library.Exporters.Scripts;
using AssetRipper.Library.Exporters.Shaders;
using AssetRipper.Library.Exporters.Terrains;
using AssetRipper.Library.Exporters.Textures;
using AssetRipper.Library.Exporters.TypeTrees;
using AssetRipper.Library.Processors;
using AssetRipper.Library.Processors.AnimatorControllers;
using AssetRipper.Library.Processors.Assemblies;
using AssetRipper.Library.Processors.PrefabOutlining;
using AssetRipper.Library.Processors.StaticMeshes;
using AssetRipper.Library.Processors.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_152;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_238;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Library
{
	public class Ripper
	{
		private GameStructure? gameStructure;

		public Ripper() : this(new()) { }

		public Ripper(LibraryConfiguration configuration)
		{
			Settings = configuration;
		}

		public GameStructure GameStructure => gameStructure ?? throw new NullReferenceException(nameof(GameStructure));
		/// <summary>
		/// Needs to be set before loading assets to ensure predictable behavior
		/// </summary>
		public LibraryConfiguration Settings { get; }
		private bool ExportersInitialized { get; set; }
		private List<IPostExporter> PostExporters { get; } = new();

		public GameStructure Load(IReadOnlyList<string> paths)
		{
			ResetData();
			if (paths.Count == 1)
			{
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths[0]}");
			}
			else
			{
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths.Count} paths...");
			}

			TaskManager.WaitUntilAllCompleted();

			gameStructure = GameStructure.Load(paths, Settings);
			TaskManager.WaitUntilAllCompleted();
			Logger.Info(LogCategory.General, "Finished reading files");

			TaskManager.WaitUntilAllCompleted();

			Logger.Info(LogCategory.General, "Processing assemblies...");

			if (Settings.ScriptContentLevel == ScriptContentLevel.Level1)
			{
				new MethodStubbingProcessor().Process(GameStructure.AssemblyManager);
			}

			Logger.Info(LogCategory.General, "Processing loaded assets...");
			UnityVersion version = GameStructure.GetMaxUnityVersion();
			foreach (IAssetProcessor processor in GetProcessors())
			{
				processor.Process(GameStructure.FileCollection, version);
			}
			TaskManager.WaitUntilAllCompleted();
			Logger.Info(LogCategory.General, "Finished processing assets");

			return GameStructure;

			IEnumerable<IAssetProcessor> GetProcessors()
			{
				yield return new SceneGuidProcessor();
				yield return new TerrainTextureProcessor();
				yield return new LightingDataProcessor();
				yield return new AnimatorControllerProcessor();
				yield return new EditorFormatProcessor(Settings.BundledAssetsExportMode);
				if (Settings.EnableStaticMeshSeparation)
				{
					yield return new StaticMeshProcessor();
				}
				if (Settings.EnablePrefabOutlining)
				{
					yield return new PrefabOutliningProcessor();
				}
				yield return new SpriteProcessor();
			}
		}

		public IEnumerable<IUnityObjectBase> FetchLoadedAssets()
		{
			if (GameStructure == null)
			{
				throw new NullReferenceException("GameStructure cannot be null");
			}

			if (GameStructure.FileCollection == null)
			{
				throw new NullReferenceException("FileCollection cannot be null");
			}

			return GameStructure.FileCollection.FetchAssets();
		}

		public void ExportProject(string exportPath) => ExportProject(exportPath, Core.Configuration.CoreConfiguration.DefaultFilter);
		public void ExportProject(string exportPath, IUnityObjectBase asset) => ExportProject(exportPath, new IUnityObjectBase[] { asset });
		public void ExportProject(string exportPath, IEnumerable<IUnityObjectBase> assets) => ExportProject(exportPath, GetFilter(assets));
		public void ExportProject<T>(string exportPath) => ExportProject(exportPath, GetFilter<T>());
		public void ExportProject(string exportPath, Type type) => ExportProject(exportPath, GetFilter(type));
		public void ExportProject(string exportPath, IEnumerable<Type> types) => ExportProject(exportPath, GetFilter(types));
		private void ExportProject(string exportPath, Func<IUnityObjectBase, bool> filter)
		{
			Logger.Info(LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			Settings.ExportRootPath = exportPath;
			Settings.Filter = filter;
			InitializeExporters();
			TaskManager.WaitUntilAllCompleted();

			Logger.Info(LogCategory.Export, "Starting pre-export");
			TaskManager.WaitUntilAllCompleted();

			Logger.Info(LogCategory.Export, "Starting export");
			GameStructure.Export(Settings);
			TaskManager.WaitUntilAllCompleted();

			Logger.Info(LogCategory.Export, "Finished exporting assets");
			TaskManager.WaitUntilAllCompleted();

			foreach (IPostExporter postExporter in PostExporters)
			{
				postExporter.DoPostExport(this);
			}
			TaskManager.WaitUntilAllCompleted();
			Logger.Info(LogCategory.Export, "Finished post-export");
		}

		public void ResetData()
		{
			PostExporters.Clear();
			ExportersInitialized = false;
			gameStructure?.Dispose();
			gameStructure = null;
		}

		public void ResetSettings() => Settings.ResetToDefaultValues();

		private static Func<IUnityObjectBase, bool> GetFilter(IEnumerable<IUnityObjectBase> assets)
		{
			if (assets == null || !assets.Any())
			{
				return CoreConfiguration.DefaultFilter;
			}
			else
			{
				return assets.Contains;
			}
		}
		private static Func<IUnityObjectBase, bool> GetFilter<T>()
		{
			return asset => asset is T;
		}
		private static Func<IUnityObjectBase, bool> GetFilter(Type type)
		{
			return asset => asset.GetType().IsAssignableTo(type);
		}
		private static Func<IUnityObjectBase, bool> GetFilter(IEnumerable<Type> types)
		{
			if (types == null || !types.Any())
			{
				return CoreConfiguration.DefaultFilter;
			}
			else
			{
				return asset => types.Any(t => asset.GetType().IsAssignableTo(t));
			}
		}

		private void InitializeExporters()
		{
			if (ExportersInitialized)
			{
				return;
			}

			OverrideNormalExporters();
			OverrideEngineExporters();

			ExportersInitialized = true;
		}

		private void OverrideNormalExporters()
		{
			//Yaml Exporters
			YamlStreamedAssetExporter streamedAssetExporter = new();
			OverrideExporter<IMesh>(streamedAssetExporter);
			OverrideExporter<ITexture2D>(streamedAssetExporter);//ICubemap also by inheritance
			OverrideExporter<ITexture3D>(streamedAssetExporter);
			OverrideExporter<ITexture2DArray>(streamedAssetExporter);
			OverrideExporter<ICubemapArray>(streamedAssetExporter);

			//Miscellaneous exporters
			OverrideExporter<ITextAsset>(new TextAssetExporter(Settings));
			OverrideExporter<IFont>(new FontAssetExporter());
			OverrideExporter<IMovieTexture>(new MovieTextureAssetExporter());
			VideoClipExporter videoClipExporter = new();
			OverrideExporter<SourceGenerated.Classes.ClassID_327.IVideoClip>(videoClipExporter);
			OverrideExporter<SourceGenerated.Classes.ClassID_329.IVideoClip>(videoClipExporter);

			//Texture exporters
			TextureAssetExporter textureExporter = new(Settings);
			OverrideExporter<ITexture2D>(textureExporter); //Texture2D and Cubemap
			OverrideExporter<ISprite>(textureExporter);
			YamlSpriteExporter spriteExporter = new();
			ConditionalOverrideExporter<ISprite>(spriteExporter, Settings.SpriteExportMode == SpriteExportMode.Yaml);
			ConditionalOverrideExporter<ISpriteAtlas>(spriteExporter, Settings.SpriteExportMode == SpriteExportMode.Yaml);

			//Shader exporters
			OverrideExporter<IShader>(new DummyShaderTextExporter());
			ConditionalOverrideExporter<IShader>(new YamlShaderExporter(), Settings.ShaderExportMode == ShaderExportMode.Yaml);
			ConditionalOverrideExporter<IShader>(new ShaderDisassemblyExporter(), Settings.ShaderExportMode == ShaderExportMode.Disassembly);
			ConditionalOverrideExporter<IShader>(new USCShaderExporter(), Settings.ShaderExportMode == ShaderExportMode.Decompile);
			OverrideExporter<IShader>(new SimpleShaderExporter());

			//Audio exporters
			OverrideExporter<IAudioClip>(new YamlAudioExporter());
			ConditionalOverrideExporter<IAudioClip>(new NativeAudioExporter(), Settings.AudioExportFormat == AudioExportFormat.Native);
			ConditionalOverrideExporter<IAudioClip>(new AudioClipExporter(Settings), AudioClipExporter.IsSupportedExportFormat(Settings.AudioExportFormat));

			//AudioMixer exporters
			AudioMixerExporter audioMixerExporter = new();
			//OverrideExporter<IAudioMixerController>(audioMixerExporter);
			//OverrideExporter<IAudioMixerGroupController>(audioMixerExporter);
			//OverrideExporter<IAudioMixerSnapshotController>(audioMixerExporter);
			//Temporarily disabled due to changes in how AssetCollections function.

			//Mesh exporters
			ConditionalOverrideExporter<IMesh>(new GlbMeshExporter(), Settings.MeshExportFormat == MeshExportFormat.Glb);

			//Model exporters
			GlbModelExporter glbModelExporter = new();
			ConditionalOverrideExporter<IComponent>(glbModelExporter, Settings.MeshExportFormat == MeshExportFormat.Glb);
			ConditionalOverrideExporter<IGameObject>(glbModelExporter, Settings.MeshExportFormat == MeshExportFormat.Glb);
			ConditionalOverrideExporter<ILevelGameManager>(glbModelExporter, Settings.MeshExportFormat == MeshExportFormat.Glb);

			//Terrain exporters
			TerrainYamlExporter terrainYamlExporter = new();
			OverrideExporter<ITerrainData>(terrainYamlExporter);
			OverrideExporter<ITexture2D>(terrainYamlExporter);
			ConditionalOverrideExporter<ITerrainData>(new TerrainHeatmapExporter(Settings), Settings.TerrainExportMode == TerrainExportMode.Heatmap);
			ConditionalOverrideExporter<ITerrainData>(new TerrainMeshExporter(), Settings.TerrainExportMode == TerrainExportMode.Mesh);

			//NavMeshData
			ConditionalOverrideExporter<INavMeshData>(new GlbNavMeshExporter(), Settings.TerrainExportMode == TerrainExportMode.Mesh);

			//Script exporters
			OverrideExporter<IMonoScript>(new ScriptExporter(GameStructure.AssemblyManager, Settings));
			ConditionalOverrideExporter<IMonoScript>(new AssemblyDllExporter(GameStructure.AssemblyManager), Settings.ScriptExportMode == ScriptExportMode.DllExportWithoutRenaming);

			//Animator Controller
			OverrideExporter<IUnityObjectBase>(new AnimatorControllerExporter());

			AddPostExporter(new ProjectVersionPostExporter());
			AddPostExporter(new PackageManifestPostExporter());
			AddPostExporter(new StreamingAssetsPostExporter());
			AddPostExporter(new TypeTreeExporter());
			AddPostExporter(new DllPostExporter());
			AddPostExporter(new PathIdMapExporter());
		}

		private void OverrideEngineExporters()
		{
			EngineAssetExporter engineExporter = new(Settings);
			OverrideExporter<IMaterial>(engineExporter);
			OverrideExporter<ITexture2D>(engineExporter);
			OverrideExporter<IMesh>(engineExporter);
			OverrideExporter<IShader>(engineExporter);
			OverrideExporter<IFont>(engineExporter);
			OverrideExporter<ISprite>(engineExporter);
			OverrideExporter<IMonoBehaviour>(engineExporter);
		}

		private void ConditionalOverrideExporter<T>(IAssetExporter exporter, bool shouldOverride)
		{
			if (shouldOverride)
			{
				OverrideExporter<T>(exporter);
			}
		}
		public void OverrideExporter<T>(IAssetExporter exporter) => GameStructure.Exporter.OverrideExporter<T>(exporter, true);
		public void AddPostExporter(IPostExporter exporter) => PostExporters.Add(exporter);
	}
}
