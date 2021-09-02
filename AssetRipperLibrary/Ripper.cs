using AssetRipper.Core;
using AssetRipper.Core.Logging;
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
		public GameStructure GameStructure { get; private set; }
		/// <summary>
		/// Needs to be set before loading assets to ensure predictable behavior
		/// </summary>
		public LibraryConfiguration Settings { get; private set; } = new();
		private bool ExportersInitialized { get; set; }

		public GameStructure Load(IReadOnlyList<string> paths)
		{
			ResetData();
			if(paths.Count == 1)
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths[0]}");
			else
				Logger.Info(LogCategory.General, "Attempting to read files...");
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

		public void ResetSettings() => Settings = new();

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

			//Cross-Platform exporters
			TextureAssetExporter textureExporter = new TextureAssetExporter(Settings);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, textureExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Cubemap, textureExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, textureExporter);
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, new ShaderAssetExporter(Settings));
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.AudioClip, new AudioClipExporter(Settings));

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
