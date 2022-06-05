using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Core.Utils;
using AssetRipper.Core.VersionHandling;
using AssetRipper.Library.Attributes;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters;
using AssetRipper.Library.Exporters.AnimationClips;
using AssetRipper.Library.Exporters.AnimatorControllers;
using AssetRipper.Library.Exporters.Audio;
using AssetRipper.Library.Exporters.Meshes;
using AssetRipper.Library.Exporters.Miscellaneous;
using AssetRipper.Library.Exporters.Scripts;
using AssetRipper.Library.Exporters.Shaders;
using AssetRipper.Library.Exporters.Terrains;
using AssetRipper.Library.Exporters.Textures;
using AssetRipper.Library.Exporters.TypeTrees;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_152;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssetRipper.Library
{
	public class Ripper
	{
		static Ripper()
		{
			VersionManager.AssetFactory = new AssetFactory();
		}

		public Ripper() : this(new()) { }

		public Ripper(LibraryConfiguration configuration)
		{
			Settings = configuration;
			LoadPlugins();
		}

		public GameStructure GameStructure { get; private set; }
		/// <summary>
		/// Needs to be set before loading assets to ensure predictable behavior
		/// </summary>
		public LibraryConfiguration Settings { get; }
		private bool ExportersInitialized { get; set; }
		private List<IPostExporter> PostExporters { get; } = new();

		public event Action OnStartLoadingGameStructure;
		public event Action OnFinishLoadingGameStructure;
		public event Action OnInitializingExporters;
		public event Action OnStartExporting;
		public event Action OnFinishExporting;

		private void LoadPlugins()
		{
			Logger.Info(LogCategory.Plugin, "Loading plugins...");
			string pluginsDirectory = ExecutingDirectory.Combine("Plugins");
			Directory.CreateDirectory(pluginsDirectory);
			List<Type> pluginTypes = new();
			foreach (string dllFile in Directory.GetFiles(pluginsDirectory, "*.dll"))
			{
				try
				{
					Logger.Info(LogCategory.Plugin, $"Found assembly at {dllFile}");
					Assembly assembly = Assembly.LoadFile(dllFile);
					foreach (RegisterPluginAttribute pluginAttr in assembly.GetCustomAttributes<RegisterPluginAttribute>())
					{
						pluginTypes.Add(pluginAttr.PluginType);
					}
				}
				catch (Exception ex)
				{
					Logger.Error(LogCategory.Plugin, $"Exception thrown while loading plugin assembly: {dllFile}", ex);
				}
			}
			foreach (Type type in pluginTypes)
			{
				try
				{
					PluginBase plugin = (PluginBase)Activator.CreateInstance(type);
					plugin.CurrentRipper = this;
					plugin.Initialize();
					Logger.Info(LogCategory.Plugin, $"Initialized plugin: {plugin.Name}");
				}
				catch (Exception ex)
				{
					Logger.Error(LogCategory.Plugin, $"Exception thrown while initializing plugin: {type?.FullName ?? "<null>"}", ex);
				}
			}
			Logger.Info(LogCategory.Plugin, "Finished loading plugins.");
		}

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

			OnStartLoadingGameStructure?.Invoke();
			TaskManager.WaitUntilAllCompleted();

			GameStructure = GameStructure.Load(paths, Settings);
			TaskManager.WaitUntilAllCompleted();
			Logger.Info(LogCategory.General, "Finished reading files");

			OnFinishLoadingGameStructure?.Invoke();
			TaskManager.WaitUntilAllCompleted();
			return GameStructure;
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
			OnStartExporting?.Invoke();
			TaskManager.WaitUntilAllCompleted();

			Logger.Info(LogCategory.Export, "Starting export");
			GameStructure.Export(Settings);
			TaskManager.WaitUntilAllCompleted();

			Logger.Info(LogCategory.Export, "Finished exporting assets");
			OnFinishExporting?.Invoke();
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
			GameStructure?.Dispose();
			GameStructure = null;
		}

		public void ResetSettings() => Settings.ResetToDefaultValues();

		private static Func<IUnityObjectBase, bool> GetFilter(IEnumerable<IUnityObjectBase> assets)
		{
			if (assets == null || !assets.Any())
			{
				return LibraryConfiguration.DefaultFilter;
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
				return LibraryConfiguration.DefaultFilter;
			}
			else
			{
				return asset => types.Any(t => asset.GetType().IsAssignableTo(t));
			}
		}

		private void InitializeExporters()
		{
			if (GameStructure == null)
			{
				throw new NullReferenceException("GameStructure cannot be null");
			}

			if (GameStructure.FileCollection == null)
			{
				throw new NullReferenceException("FileCollection cannot be null");
			}

			if (GameStructure.Exporter == null)
			{
				throw new NullReferenceException("Project Exporter cannot be null");
			}

			if (ExportersInitialized)
			{
				return;
			}

			OverrideNormalExporters();
			OnInitializingExporters?.Invoke();
			OverrideEngineExporters();

			ExportersInitialized = true;
		}

		private void OverrideNormalExporters()
		{
			//Yaml Exporters
			YamlStreamedAssetExporter streamedAssetExporter = new YamlStreamedAssetExporter();
			OverrideExporter<IMesh>(streamedAssetExporter);
			OverrideExporter<ITexture2D>(streamedAssetExporter);//ICubemap also by inheritance
			OverrideExporter<ITexture3D>(streamedAssetExporter);
			OverrideExporter<ITexture2DArray>(streamedAssetExporter);
			OverrideExporter<ICubemapArray>(streamedAssetExporter);

			//Miscellaneous exporters
			OverrideExporter<ITextAsset>(new TextAssetExporter(Settings));
			OverrideExporter<IFont>(new FontAssetExporter());
			OverrideExporter<IMovieTexture>(new MovieTextureAssetExporter());

			//Texture exporters
			TextureAssetExporter textureExporter = new(Settings);
			OverrideExporter<ITexture2D>(textureExporter); //Texture2D and Cubemap
			OverrideExporter<ISprite>(textureExporter);

			//Shader exporters
			OverrideExporter<IShader>(new DummyShaderTextExporter(Settings));
			OverrideExporter<IShader>(new YamlShaderExporter(Settings));
			OverrideExporter<IShader>(new ShaderDisassemblyExporter(Settings));
			OverrideExporter<IShader>(new SimpleShaderExporter());

			//Audio exporters
			OverrideExporter<IAudioClip>(new YamlAudioExporter());
			OverrideExporter<IAudioClip>(new NativeAudioExporter(Settings));
			OverrideExporter<IAudioClip>(new AudioClipExporter(Settings));

			//Mesh exporters
			OverrideExporter<Core.Classes.Mesh.Mesh>(new GlbMeshExporter(Settings));
			OverrideExporter<Core.Classes.Mesh.Mesh>(new UnifiedMeshExporter(Settings));

			//Terrain exporters
			OverrideExporter<ITerrainData>(new TerrainHeatmapExporter(Settings));
			OverrideExporter<ITerrainData>(new TerrainObjExporter(Settings));

			//Script exporters
			OverrideExporter<IMonoScript>(new AssemblyDllExporter(GameStructure.FileCollection.AssemblyManager, Settings));
			OverrideExporter<IMonoScript>(new ScriptExporter(GameStructure.FileCollection.AssemblyManager, Settings));
			OverrideExporter<IMonoScript>(new SkipScriptExporter(Settings));

			//Animator Controller
			OverrideExporter<IAnimatorController>(new AnimatorControllerExporter());

			//Animation Clip
			OverrideExporter<IAnimationClip>(new AnimationClipExporter());

			AddPostExporter(new ProjectVersionPostExporter());
			AddPostExporter(new TypeTreeExporter());
			AddPostExporter(new DllPostExporter());
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

		public void OverrideExporter<T>(IAssetExporter exporter) => GameStructure.Exporter.OverrideExporter<T>(exporter, true);
		public void OverrideExporter<T>(IAssetExporter exporter, bool allowInheritance) => GameStructure.Exporter.OverrideExporter<T>(exporter, allowInheritance);
		public void AddPostExporter(IPostExporter exporter) => PostExporters.Add(exporter);
	}
}
