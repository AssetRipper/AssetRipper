using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.Library.Exporters.Audio;
using AssetRipper.Library.Exporters.Shaders;
using AssetRipper.Library.Exporters.Textures;
using System;
using System.Collections.Generic;
using AssetRipper.Core;

namespace AssetRipper.Library
{
	public class Ripper
	{
		private GameStructure GameStructure { get; set; }

		public GameStructure Load(IReadOnlyList<string> paths)
		{
			if(paths.Count == 1)
				Logger.Log(LogType.Info, LogCategory.General, $"Attempting to read files from {paths[0]}");
			else
				Logger.Log(LogType.Info, LogCategory.General, "Attempting to read files...");
			GameStructure = GameStructure.Load(paths);
			Logger.Log(LogType.Info, LogCategory.General, "Finished reading files");
			return GameStructure;
		}

		public void Export(string exportPath)
		{
			Logger.Log(LogType.Info, LogCategory.General, $"Attempting to export assets to {exportPath}...");
			if (GameStructure == null) throw new NullReferenceException("GameStructure cannot be null");
			if (GameStructure.FileCollection == null) throw new NullReferenceException("FileCollection cannot be null");
			if (GameStructure.FileCollection.Exporter == null) throw new NullReferenceException("Project Exporter cannot be null");

			//Core Exporters
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.TextAsset, new TextAssetExporter());
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, new FontAssetExporter());
			GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MovieTexture, new MovieTextureAssetExporter());

			//Library Exporters
			if (OperatingSystem.IsWindows())
			{
				TextureAssetExporter textureExporter = new TextureAssetExporter();
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Cubemap, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.AudioClip, new AudioAssetExporter());
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, new ShaderAssetExporter());
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

			GameStructure.Export(exportPath);
			Logger.Log(LogType.Info, LogCategory.General, "Finished exporting assets");
		}
	}
}
