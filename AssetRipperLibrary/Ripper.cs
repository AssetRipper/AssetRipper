using AssetRipper.Converters.Project.Exporter;
using AssetRipper.Converters.Project.Exporter.Engine;
using AssetRipper.Logging;
using AssetRipper.Parser.Asset;
using AssetRipper.Structure.GameStructure;
using AssetRipperLibrary.Exporters.Audio;
using AssetRipperLibrary.Exporters.Shaders;
using AssetRipperLibrary.Exporters.Textures;
using System;
using System.Collections.Generic;

namespace AssetRipperLibrary
{
	public class Ripper
	{
		private GameStructure GameStructure { get; set; }

		public GameStructure Load(IReadOnlyList<string> paths)
		{
			try
			{
				Logger.Log(LogType.Info, LogCategory.General, "Attempting to read files...");
				GameStructure = GameStructure.Load(paths);
				Logger.Log(LogType.Info, LogCategory.General, "Finished reading files");
			}
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
			return GameStructure;
		}

		public void Export(string exportPath)
		{
			try
			{
				Logger.Log(LogType.Info, LogCategory.General, $"Attempting to export assets to {exportPath}...");
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
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
		}
	}
}
