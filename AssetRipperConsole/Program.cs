using AssetRipper;
using AssetRipper.Converters;
using AssetRipper.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Object = AssetRipper.Classes.Object;

namespace AssetRipperConsole
{
	public class Program
	{
		public static bool AssetSelector(Object asset)
		{
			return true;
		}

		public static void Main(string[] args)
		{
			Logger.Add(new ConsoleLogger());
			Logger.Add(new FileLogger());

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

				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.TextAsset, new TextAssetExporter());
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, new FontAssetExporter());
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MovieTexture, new MovieTextureAssetExporter());

#if DEBUG
				EngineAssetExporter engineExporter = new EngineAssetExporter();
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Material, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Mesh, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MonoBehaviour, engineExporter);
#endif

				GameStructure.Export(exportPath, AssetSelector);
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
