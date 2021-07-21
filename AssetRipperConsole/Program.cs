using AssetRipper.Converters.Project.Exporter;
using AssetRipper.Converters.Project.Exporter.Engine;
using AssetRipper.IO.MultiFile;
using AssetRipper.Logging;
using AssetRipper.Parser.Asset;
using AssetRipper.Structure.GameStructure;
using AssetRipper.Utils;
using AssetRipperLibrary.Exporters.Audio;
using AssetRipperLibrary.Exporters.Texture;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipperConsole
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Logger.Add(new ConsoleLogger());
			Logger.Add(new FileLogger("AssetRipperConsole.log"));

			if (args.Length == 0)
			{
				Console.WriteLine("No arguments");
				Console.ReadKey();
				return;
			}

			foreach (string arg in args)
			{
				if (MultiFileStream.Exists(arg))
				{
					continue;
				}
				if (DirectoryUtils.Exists(arg))
				{
					continue;
				}
				Console.WriteLine(MultiFileStream.IsMultiFile(arg) ?
					$"File '{arg}' doesn't have all parts for combining" :
					$"Neither file nor directory with path '{arg}' exists");
				Console.ReadKey();
				return;
			}

			Program program = new Program();
			program.Load(args);
			Console.ReadKey();
		}

		public void Load(IReadOnlyList<string> args)
		{
#if !DEBUG
			try
#endif
			{
				GameStructure = GameStructure.Load(args);

				string exportPath = Path.Combine("Ripped", GameStructure.Name);
				PrepareExportDirectory(exportPath);

				//Core Exporters
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.TextAsset, new TextAssetExporter());
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, new FontAssetExporter());
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MovieTexture, new MovieTextureAssetExporter());

				//Library Exporters
				TextureAssetExporter textureExporter = new TextureAssetExporter();
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Cubemap, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, textureExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.AudioClip, new AudioAssetExporter());

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
				Logger.Log(LogType.Info, LogCategory.General, "Finished");
			}
#if !DEBUG
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
#endif
		}

		private static void PrepareExportDirectory(string path)
		{
			if (DirectoryUtils.Exists(path))
			{
				DirectoryUtils.Delete(path, true);
			}
		}

		private GameStructure GameStructure { get; set; }
	}
}
