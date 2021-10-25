using AssetRipper.Core;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Attributes;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters.Audio;
using AssetRipper.Library.Exporters.Meshes;
using AssetRipper.Library.Exporters.Miscellaneous;
using AssetRipper.Library.Exporters.Scripts;
using AssetRipper.Library.Exporters.Shaders;
using AssetRipper.Library.Exporters.Terrains;
using AssetRipper.Library.Exporters.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityObject = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Library
{
	public class Ripper
	{
		public Ripper() => LoadPlugins();

		public GameStructure GameStructure { get; private set; }
		/// <summary>
		/// Needs to be set before loading assets to ensure predictable behavior
		/// </summary>
		public LibraryConfiguration Settings { get; } = new();
		private bool ExportersInitialized { get; set; }

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
			var pluginTypes = new List<Type>();
			foreach(string dllFile in Directory.GetFiles(pluginsDirectory, "*.dll"))
			{
				try
				{
					Logger.Info(LogCategory.Plugin, $"Found assembly at {dllFile}");
					var assembly = Assembly.LoadFile(dllFile);
					foreach(var pluginAttr in assembly.GetCustomAttributes<RegisterPluginAttribute>())
					{
						pluginTypes.Add(pluginAttr.PluginType);
					}
				}
				catch(Exception ex)
				{
					Logger.Error(LogCategory.Plugin, $"Exception thrown while loading plugin assembly: {dllFile}", ex);
				}
			}
			foreach(var type in pluginTypes)
			{
				try
				{
					var plugin = (PluginBase)Activator.CreateInstance(type);
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
			if(paths.Count == 1)
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths[0]}");
			else
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths.Count} paths...");
			OnStartLoadingGameStructure?.Invoke();
			GameStructure = GameStructure.Load(paths, Settings);
			Logger.Info(LogCategory.General, "Finished reading files");
			OnFinishLoadingGameStructure?.Invoke();
			return GameStructure;
		}

		public IEnumerable<UnityObjectBase> FetchLoadedAssets()
		{
			if (GameStructure == null) throw new NullReferenceException("GameStructure cannot be null");
			if (GameStructure.FileCollection == null) throw new NullReferenceException("FileCollection cannot be null");
			return GameStructure.FileCollection.FetchAssets();
		}

		public void ExportFile(string exportPath, UnityObjectBase asset) => throw new NotImplementedException();
		public void ExportFile(string exportPath, IEnumerable<UnityObjectBase> assets) => throw new NotImplementedException();

		public void ExportProject(string exportPath) => ExportProject(exportPath, new UnityObjectBase[0]);
		public void ExportProject(string exportPath, UnityObjectBase asset) => ExportProject(exportPath, new UnityObjectBase[] { asset });
		public void ExportProject(string exportPath, IEnumerable<UnityObjectBase> assets)
		{
			Logger.Info(LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			List<UnityObjectBase> list = new List<UnityObjectBase>(assets ?? new UnityObjectBase[0]);
			Settings.ExportPath = exportPath;
			Settings.Filter = list.Count == 0 ? LibraryConfiguration.DefaultFilter : GetFilter(list);
			InitializeExporters();
			Logger.Info(LogCategory.Export, "Starting pre-export");
			OnStartExporting?.Invoke();
			Logger.Info(LogCategory.Export, "Starting export");
			GameStructure.Export(Settings);
			Logger.Info(LogCategory.Export, "Finished exporting assets");
			OnFinishExporting?.Invoke();
			Logger.Info(LogCategory.Export, "Finished post-export");
		}

		public void ResetData()
		{
			ExportersInitialized = false;
			GameStructure?.Dispose();
			GameStructure = null;
		}

		public void ResetSettings() => Settings.ResetToDefaultValues();

		private static Func<UnityObjectBase, bool> GetFilter(List<UnityObjectBase> assets)
		{
			if (assets == null) throw new ArgumentNullException(nameof(assets));
			return assets.Contains;
		}

		private void InitializeExporters()
		{
			if (GameStructure == null) throw new NullReferenceException("GameStructure cannot be null");
			if (GameStructure.FileCollection == null) throw new NullReferenceException("FileCollection cannot be null");
			if (GameStructure.FileCollection.Exporter == null) throw new NullReferenceException("Project Exporter cannot be null");
			if (ExportersInitialized)
				return;

			OverrideNormalExporters();
			OnInitializingExporters?.Invoke();
			OverrideEngineExporters();

			ExportersInitialized = true;
		}

		private void OverrideNormalExporters()
		{
			//Texture exporters
			TextureAssetExporter textureExporter = new TextureAssetExporter(Settings);
			OverrideExporter(ClassIDType.Texture2D, textureExporter);
			OverrideExporter(ClassIDType.Cubemap, textureExporter);
			OverrideExporter(ClassIDType.Sprite, textureExporter);

			//Shader exporters
			OverrideExporter(ClassIDType.Shader, new DummyShaderTextExporter());

			//Audio exporters
			OverrideExporter(ClassIDType.AudioClip, new NativeAudioExporter());
			OverrideExporter(ClassIDType.AudioClip, new FmodAudioExporter(Settings));
			OverrideExporter(ClassIDType.AudioClip, new AudioClipExporter(Settings));

			//Mesh exporters
			OverrideExporter(ClassIDType.Mesh, new GlbMeshExporter(Settings));
			OverrideExporter(ClassIDType.Mesh, new StlMeshExporter(Settings));
			OverrideExporter(ClassIDType.Mesh, new ObjMeshExporter(Settings));

			//Terrain exporters
			OverrideExporter(ClassIDType.TerrainData, new TerrainHeatmapExporter(Settings));
			OverrideExporter(ClassIDType.TerrainData, new TerrainObjExporter(Settings));

			//Script exporters
			OverrideExporter(ClassIDType.MonoScript, new ScriptExporter(GameStructure.FileCollection.AssemblyManager, Settings));
			OverrideExporter(ClassIDType.MonoScript, new AssemblyDllExporter(GameStructure.FileCollection.AssemblyManager, Settings));

			//Miscellaneous exporters
			OverrideExporter(ClassIDType.TextAsset, new TextAssetExporter(Settings));
			OverrideExporter(ClassIDType.Font, new FontAssetExporter());
			OverrideExporter(ClassIDType.MovieTexture, new MovieTextureAssetExporter());
		}

		private void OverrideEngineExporters()
		{
			EngineAssetExporter engineExporter = new EngineAssetExporter(Settings);
			OverrideExporter(ClassIDType.Material, engineExporter);
			OverrideExporter(ClassIDType.Texture2D, engineExporter);
			OverrideExporter(ClassIDType.Mesh, engineExporter);
			OverrideExporter(ClassIDType.Shader, engineExporter);
			OverrideExporter(ClassIDType.Font, engineExporter);
			OverrideExporter(ClassIDType.Sprite, engineExporter);
			OverrideExporter(ClassIDType.MonoBehaviour, engineExporter);
		}

		private void OverrideExporter(ClassIDType classID, IAssetExporter exporter) => GameStructure.FileCollection.Exporter.OverrideExporter(classID, exporter);
	}
}
