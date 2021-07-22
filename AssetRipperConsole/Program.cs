using AssetRipper.IO.MultiFile;
using AssetRipper.Logging;
using AssetRipper.Structure.GameStructure;
using AssetRipper.Utils;
using AssetRipperLibrary;
using System;
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

			Logger.LogSystemInformation();

			Ripper ripper = new Ripper();
			GameStructure gameStructure = ripper.Load(args);
			string exportPath = Path.Combine("Ripped", gameStructure.Name);
			PrepareExportDirectory(exportPath);
			ripper.Export(exportPath);
			Console.ReadKey();
		}

		private static void PrepareExportDirectory(string path)
		{
			if (DirectoryUtils.Exists(path))
			{
				DirectoryUtils.Delete(path, true);
			}
		}
	}
}
