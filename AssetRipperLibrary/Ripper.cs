using AssetRipper.Core;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters.Audio;
using AssetRipper.Library.Exporters.Shaders;
using AssetRipper.Library.Exporters.Textures;
using System;
using System.Collections.Generic;
using UnityObject = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Library
{
	public class Ripper
	{
		public GameStructure GameStructure { get; set; }
		public LibraryConfiguration Settings { get; } = new();
		private bool ExportersInitialized { get; set; }

		public GameStructure Load(IReadOnlyList<string> paths)
		{
			ExportersInitialized = false;
			if(paths.Count == 1)
				Logger.Log(LogType.Info, LogCategory.General, $"Attempting to read files from {paths[0]}");
			else
				Logger.Log(LogType.Info, LogCategory.General, "Attempting to read files...");
			GameStructure = GameStructure.Load(paths);
			Logger.Log(LogType.Info, LogCategory.General, "Finished reading files");
			return GameStructure;
		}

		public IEnumerable<UnityObject> FetchLoadedAssets()
		{
			if (GameStructure == null) throw new NullReferenceException("GameStructure cannot be null");
			if (GameStructure.FileCollection == null) throw new NullReferenceException("FileCollection cannot be null");
			return GameStructure.FileCollection.FetchAssets();
		}

		public void Export(string exportPath)
		{
			Logger.Log(LogType.Info, LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			Settings.ExportPath = exportPath;
			Settings.Filter = LibraryConfiguration.DefaultFilter;
			InitializeExporters();
			GameStructure.Export(Settings);
			Logger.Log(LogType.Info, LogCategory.Export, "Finished exporting assets");
		}

		public void Export(string exportPath, UnityObject asset) => Export(exportPath, new UnityObject[] { asset });
		public void Export(string exportPath, IEnumerable<UnityObject> assets)
		{
			Logger.Log(LogType.Info, LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			List<UnityObject> list = new List<UnityObject>(assets);
			Settings.ExportPath = exportPath;
			Settings.Filter = GetFilter(list);
			InitializeExporters();
			GameStructure.Export(Settings);
			Logger.Log(LogType.Info, LogCategory.Export, "Finished exporting assets");
		}

		private static Func<UnityObject, bool> GetFilter(List<UnityObject> assets)
		{
			if (assets == null) throw new ArgumentNullException(nameof(assets));
			return (UnityObject obj) => assets.Contains(obj);
		}

		private void InitializeExporters()
		{
			if (GameStructure == null) throw new NullReferenceException("GameStructure cannot be null");
			if (GameStructure.FileCollection == null) throw new NullReferenceException("FileCollection cannot be null");
			if (GameStructure.FileCollection.Exporter == null) throw new NullReferenceException("Project Exporter cannot be null");
			if (ExportersInitialized)
				return;

			//Library Exporters
			if (OperatingSystem.IsWindows())
			{
				TextureAssetExporter textureExporter = new TextureAssetExporter();
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Cubemap, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.AudioClip, new AudioAssetExporter());
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, new ShaderAssetExporter(Settings));
			}

			//Engine Exporters
			EngineAssetExporter engineExporter = new EngineAssetExporter();
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Material, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Mesh, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, engineExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MonoBehaviour, engineExporter);

			ExportersInitialized = true;
		}
	}
}
