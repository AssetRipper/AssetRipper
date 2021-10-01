using AssetRipper.Core;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.Structure.GameStructure;
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
using UnityObject = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Library
{
	public class Ripper
	{
		public GameStructure GameStructure { get; private set; }
		/// <summary>
		/// Needs to be set before loading assets to ensure predictable behavior
		/// </summary>
		public LibraryConfiguration Settings { get; } = new();
		private bool ExportersInitialized { get; set; }

		public GameStructure Load(IReadOnlyList<string> paths)
		{
			ResetData();
			if(paths.Count == 1)
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths[0]}");
			else
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths.Count} paths...");
			GameStructure = GameStructure.Load(paths, Settings);
			Logger.Info(LogCategory.General, "Finished reading files");
			return GameStructure;
		}

		public IEnumerable<UnityObject> FetchLoadedAssets()
		{
			if (GameStructure == null) throw new NullReferenceException("GameStructure cannot be null");
			if (GameStructure.FileCollection == null) throw new NullReferenceException("FileCollection cannot be null");
			return GameStructure.FileCollection.FetchAssets();
		}

		public void ExportFile(string exportPath, UnityObject asset) => throw new NotImplementedException();
		public void ExportFile(string exportPath, IEnumerable<UnityObject> assets) => throw new NotImplementedException();

		public void ExportProject(string exportPath, UnityObject asset) => ExportProject(exportPath, new UnityObject[] { asset });
		public void ExportProject(string exportPath, IEnumerable<UnityObject> assets)
		{
			Logger.Info(LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			List<UnityObject> list = new List<UnityObject>(assets);
			Settings.ExportPath = exportPath;
			Settings.Filter = GetFilter(list);
			InitializeExporters();
			GameStructure.Export(Settings);
			Logger.Info(LogCategory.Export, "Finished exporting assets");
		}
		public void ExportProject(string exportPath)
		{
			Logger.Info(LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			Settings.ExportPath = exportPath;
			Settings.Filter = LibraryConfiguration.DefaultFilter;
			InitializeExporters();
			GameStructure.Export(Settings);
			Logger.Info(LogCategory.Export, "Finished exporting assets");
		}

		public void ResetData()
		{
			ExportersInitialized = false;
			GameStructure?.Dispose();
			GameStructure = null;
		}

		public void ResetSettings() => Settings.ResetToDefaultValues();

		private static Func<UnityObject, bool> GetFilter(List<UnityObject> assets)
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

			//Engine exporters
			EngineAssetExporter engineExporter = new EngineAssetExporter(Settings);
			OverrideExporter(ClassIDType.Material, engineExporter);
			OverrideExporter(ClassIDType.Texture2D, engineExporter);
			OverrideExporter(ClassIDType.Mesh, engineExporter);
			OverrideExporter(ClassIDType.Shader, engineExporter);
			OverrideExporter(ClassIDType.Font, engineExporter);
			OverrideExporter(ClassIDType.Sprite, engineExporter);
			OverrideExporter(ClassIDType.MonoBehaviour, engineExporter);

			ExportersInitialized = true;
		}

		private void OverrideExporter(ClassIDType classID, IAssetExporter exporter) => GameStructure.FileCollection.Exporter.OverrideExporter(classID, exporter);
	}
}
