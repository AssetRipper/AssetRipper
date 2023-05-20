using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Export.UnityProjects.AnimatorControllers;
using AssetRipper.Export.UnityProjects.Audio;
using AssetRipper.Export.UnityProjects.AudioMixers;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Meshes;
using AssetRipper.Export.UnityProjects.Miscellaneous;
using AssetRipper.Export.UnityProjects.Models;
using AssetRipper.Export.UnityProjects.NavMeshes;
using AssetRipper.Export.UnityProjects.PathIdMapping;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Export.UnityProjects.Project.Exporters.Engine;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Export.UnityProjects.TypeTrees;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.GameStructure;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Processing;
using AssetRipper.Processing.AnimatorControllers;
using AssetRipper.Processing.Assemblies;
using AssetRipper.Processing.AudioMixers;
using AssetRipper.Processing.PrefabOutlining;
using AssetRipper.Processing.Scenes;
using AssetRipper.Processing.StaticMeshes;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1113;
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
using AssetRipper.SourceGenerated.Classes.ClassID_240;
using AssetRipper.SourceGenerated.Classes.ClassID_244;
using AssetRipper.SourceGenerated.Classes.ClassID_272;
using AssetRipper.SourceGenerated.Classes.ClassID_273;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Classes.ClassID_72;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using System.Text;

namespace AssetRipper.Export.UnityProjects
{
	public class Ripper
	{
		private GameStructure? gameStructure;

		public Ripper() : this(new()) { }

		public Ripper(LibraryConfiguration configuration)
		{
			Settings = configuration;
		}

		public bool IsLoaded => gameStructure != null;

		public GameStructure GameStructure
		{
			[MemberNotNull(nameof(gameStructure))]
			get
			{
				ThrowIfGameStructureIsNull();
				return gameStructure;
			}
		}

		/// <summary>
		/// Needs to be set before loading assets to ensure predictable behavior
		/// </summary>
		public LibraryConfiguration Settings { get; }

		public void ResetData()
		{
			gameStructure?.Dispose();
			gameStructure = null;
		}

		[MemberNotNull(nameof(gameStructure))]
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

			gameStructure = GameStructure.Load(paths, Settings);
			Logger.Info(LogCategory.General, "Finished reading files");

			Logger.Info(LogCategory.General, "Processing assemblies...");
			if (Settings.ScriptContentLevel == ScriptContentLevel.Level1)
			{
				new MethodStubbingProcessor().Process(GameStructure.AssemblyManager);
			}

			Logger.Info(LogCategory.General, "Processing loaded assets...");
			UnityVersion version = gameStructure.FileCollection.GetMaxUnityVersion();
			foreach (IAssetProcessor processor in GetProcessors())
			{
				processor.Process(GameStructure.FileCollection, version);
			}
			Logger.Info(LogCategory.General, "Finished processing assets");

			return GameStructure;

			IEnumerable<IAssetProcessor> GetProcessors()
			{
				yield return new SceneDefinitionProcessor();
				yield return new MainAssetProcessor();
				yield return new LightingDataProcessor();
				yield return new AnimatorControllerProcessor();
				yield return new AudioMixerProcessor();
				yield return new EditorFormatProcessor(Settings.BundledAssetsExportMode);
				if (Settings.EnableStaticMeshSeparation)
				{
					yield return new StaticMeshProcessor();
				}
				if (Settings.EnablePrefabOutlining)
				{
					yield return new PrefabOutliningProcessor();
				}
				yield return new PrefabProcessor();
				yield return new SpriteProcessor();
			}
		}

		public IEnumerable<IUnityObjectBase> FetchLoadedAssets()
		{
			return GameStructure.FileCollection.FetchAssets();
		}

		[MemberNotNull(nameof(gameStructure))]
		private void ThrowIfGameStructureIsNull()
		{
			if (gameStructure == null)
			{
				throw new NullReferenceException("GameStructure cannot be null");
			}
		}

		[MemberNotNull(nameof(gameStructure))]
		public void ExportProject(string exportPath, Action<ProjectExporter>? onBeforeExport = null) => ExportProject(exportPath, CoreConfiguration.DefaultFilter, onBeforeExport);

		[MemberNotNull(nameof(gameStructure))]
		public void ExportProject(string exportPath, Func<IUnityObjectBase, bool> filter, Action<ProjectExporter>? onBeforeExport)
		{
			Logger.Info(LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			ThrowIfGameStructureIsNull();

			Logger.Info(LogCategory.Export, "Starting export");
			Logger.Info(LogCategory.Export, $"Game files have these Unity versions:{GetListOfVersions(gameStructure.FileCollection)}");
			UnityVersion version = gameStructure.FileCollection.GetMaxUnityVersion();
			Logger.Info(LogCategory.Export, $"Exporting to Unity version {version}");

			Settings.ExportRootPath = exportPath;
			Settings.Filter = filter;
			Settings.SetProjectSettings(version, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);

			{
				ProjectExporter projectExporter = new();
				onBeforeExport?.Invoke(projectExporter);
				InitializeExporters(projectExporter);
				projectExporter.Export(gameStructure.FileCollection, Settings);
			}
			Logger.Info(LogCategory.Export, "Finished exporting assets");

			foreach (IPostExporter postExporter in GetPostExporters())
			{
				postExporter.DoPostExport(this);
			}
			Logger.Info(LogCategory.Export, "Finished post-export");

			static string GetListOfVersions(GameBundle gameBundle)
			{
				StringBuilder sb = new();
				foreach (UnityVersion version in gameBundle.FetchAssetCollections().Select(s => s.Version).Distinct())
				{
					sb.Append(' ');
					sb.Append(version.ToString());
				}
				return sb.ToString();
			}

			static IEnumerable<IPostExporter> GetPostExporters()
			{
				yield return new ProjectVersionPostExporter();
				yield return new PackageManifestPostExporter();
				yield return new StreamingAssetsPostExporter();
				yield return new TypeTreeExporter();
				yield return new DllPostExporter();
				yield return new PathIdMapExporter();
			}
		}

		private void InitializeExporters(ProjectExporter projectExporter)
		{
			OverrideNormalExporters(projectExporter);
			if (!Settings.IgnoreEngineAssets)
			{
				OverrideEngineExporters(projectExporter);
			}
		}

		private void OverrideNormalExporters(ProjectExporter projectExporter)
		{
			//Yaml Exporters
			YamlStreamedAssetExporter streamedAssetExporter = new();
			projectExporter.OverrideExporter<IMesh>(streamedAssetExporter);
			projectExporter.OverrideExporter<ITexture2D>(streamedAssetExporter);//ICubemap also by inheritance
			projectExporter.OverrideExporter<ITexture3D>(streamedAssetExporter);
			projectExporter.OverrideExporter<ITexture2DArray>(streamedAssetExporter);
			projectExporter.OverrideExporter<ICubemapArray>(streamedAssetExporter);

			//Miscellaneous exporters
			projectExporter.OverrideExporter<ITextAsset>(new TextAssetExporter(Settings));
			FontAssetExporter fontAssetExporter = new FontAssetExporter();
			projectExporter.OverrideExporter<IFont>(fontAssetExporter);
			projectExporter.OverrideExporter<IMaterial>(fontAssetExporter);
			projectExporter.OverrideExporter<IMovieTexture>(new MovieTextureAssetExporter());
			VideoClipExporter videoClipExporter = new();
			projectExporter.OverrideExporter<SourceGenerated.Classes.ClassID_327.IVideoClip>(videoClipExporter);
			projectExporter.OverrideExporter<SourceGenerated.Classes.ClassID_329.IVideoClip>(videoClipExporter);

			//Texture exporters
			TextureAssetExporter textureExporter = new(Settings);
			projectExporter.OverrideExporter<ITexture2D>(textureExporter); //Texture2D and Cubemap
			projectExporter.OverrideExporter<ISprite>(textureExporter);
			if (Settings.SpriteExportMode == SpriteExportMode.Yaml)
			{
				YamlSpriteExporter spriteExporter = new();
				projectExporter.OverrideExporter<ISprite>(spriteExporter);
				projectExporter.OverrideExporter<ISpriteAtlas>(spriteExporter);
			}

			//Shader exporters
			projectExporter.OverrideExporter<IShader>(Settings.ShaderExportMode switch
			{
				ShaderExportMode.Yaml => new YamlShaderExporter(),
				ShaderExportMode.Disassembly => new ShaderDisassemblyExporter(),
				ShaderExportMode.Decompile => new USCShaderExporter(),
				_ => new DummyShaderTextExporter(),
			});
			projectExporter.OverrideExporter<IShader>(new SimpleShaderExporter());

			//Audio exporters
			projectExporter.OverrideExporter<IAudioClip>(new YamlAudioExporter());
			if (Settings.AudioExportFormat == AudioExportFormat.Native)
			{
				projectExporter.OverrideExporter<IAudioClip>(new NativeAudioExporter());
			}
			if (AudioClipExporter.IsSupportedExportFormat(Settings.AudioExportFormat))
			{
				projectExporter.OverrideExporter<IAudioClip>(new AudioClipExporter(Settings));
			}

			//AudioMixer exporters
			AudioMixerExporter audioMixerExporter = new();
			projectExporter.OverrideExporter<IAudioMixer>(audioMixerExporter);
			projectExporter.OverrideExporter<IAudioMixerEffectController>(audioMixerExporter);
			projectExporter.OverrideExporter<IAudioMixerGroup>(audioMixerExporter);
			projectExporter.OverrideExporter<IAudioMixerSnapshot>(audioMixerExporter);

			//Mesh and Model exporters
			if (Settings.MeshExportFormat == MeshExportFormat.Glb)
			{
				projectExporter.OverrideExporter<IMesh>(new GlbMeshExporter());
				GlbModelExporter glbModelExporter = new();
				projectExporter.OverrideExporter<IComponent>(glbModelExporter);
				projectExporter.OverrideExporter<IGameObject>(glbModelExporter);
				projectExporter.OverrideExporter<ILevelGameManager>(glbModelExporter);
			}

			//Terrain and NavMesh exporters
			switch (Settings.TerrainExportMode)
			{
				case TerrainExportMode.Heatmap:
					projectExporter.OverrideExporter<ITerrainData>(new TerrainHeatmapExporter(Settings));
					break;
				case TerrainExportMode.Mesh:
					projectExporter.OverrideExporter<ITerrainData>(new TerrainMeshExporter());
					projectExporter.OverrideExporter<INavMeshData>(new GlbNavMeshExporter());
					break;
				default:
					TerrainYamlExporter terrainYamlExporter = new();
					projectExporter.OverrideExporter<ITerrainData>(terrainYamlExporter);
					projectExporter.OverrideExporter<ITexture2D>(terrainYamlExporter);
					break;
			}

			//Script exporters
			projectExporter.OverrideExporter<IMonoScript>(Settings.ScriptExportMode switch
			{
				ScriptExportMode.DllExportWithoutRenaming => new AssemblyDllExporter(GameStructure.AssemblyManager),
				_ => new ScriptExporter(GameStructure.AssemblyManager, Settings),
			});

			//Animator Controller
			projectExporter.OverrideExporter<IUnityObjectBase>(new AnimatorControllerExporter());
		}

		private void OverrideEngineExporters(ProjectExporter projectExporter)
		{
			EngineAssetExporter engineExporter = new(Settings);
			projectExporter.OverrideExporter<IMaterial>(engineExporter);
			projectExporter.OverrideExporter<ITexture2D>(engineExporter);
			projectExporter.OverrideExporter<IMesh>(engineExporter);
			projectExporter.OverrideExporter<IShader>(engineExporter);
			projectExporter.OverrideExporter<IComputeShader>(engineExporter);
			projectExporter.OverrideExporter<IMonoBehaviour>(engineExporter);
			projectExporter.OverrideExporter<IMonoScript>(engineExporter);
			projectExporter.OverrideExporter<IFont>(engineExporter);
			projectExporter.OverrideExporter<ISprite>(engineExporter);
			projectExporter.OverrideExporter<ILightmapParameters>(engineExporter);
		}
	}
}
