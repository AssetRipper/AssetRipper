using AssetRipper.IO.MultiFile;
using AssetRipper.Logging;
using AssetRipper.Structure.GameStructure;
using AssetRipper.Utils;
using AssetRipperLibrary;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipperConsole
{
	public class Program
	{
		private const string DefaultLogFileName = "AssetRipperConsole.log";

		internal class Options
		{
			[Value(0, Required = true, HelpText = "Input files or directory to export.")]
			public IReadOnlyList<string> FilesToExport { get; set; }

			[Option('o', "output", HelpText = "Directory to export to. Will be cleared if already exists.")]
			public DirectoryInfo OutputDirectory { get; set; }

			[Option("logFile", HelpText = "(Default: " + DefaultLogFileName + ") File to log to.")]
			public FileInfo LogFile { get; set; }

			[Option('q', "quit", Default = false, HelpText = "Close console after export.")]
			public bool Quit { get; set; }
		}

		public static void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<Options>(args)
				.WithParsed(options =>
				{
					if (ValidateOptions(options))
					{
						Run(options);
					}
					else
					{
						Environment.ExitCode = 1;
					}

					if (!options.Quit)
					{
						Console.ReadKey();
					}
				});
		}

		private static bool ValidateOptions(Options options)
		{
			foreach (string arg in options.FilesToExport)
			{
				if (MultiFileStream.Exists(arg))
				{
					continue;
				}

				if (DirectoryUtils.Exists(arg))
				{
					continue;
				}

				Console.WriteLine(MultiFileStream.IsMultiFile(arg)
					? $"File '{arg}' doesn't have all parts for combining"
					: $"Neither file nor directory with path '{arg}' exists");

				return false;
			}

			try
			{
				if (options.LogFile == null)
					options.LogFile = new FileInfo(Path.Combine(DirectoryUtils.GetExecutingDirectory(), DefaultLogFileName));
				if (options.OutputDirectory == null)
					options.OutputDirectory = new DirectoryInfo(Path.Combine(DirectoryUtils.GetExecutingDirectory(), "Ripped"));
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Failed to initialize the output and log paths. {ex}");
				return false;
			}

			return true;
		}

		private static void Run(Options options)
		{
			Logger.Add(new ConsoleLogger());
			Logger.Add(new FileLogger(options.LogFile.FullName));
#if PLATFORM_X64
			Logger.LogSystemInformation("AssetRipper Console Version", "x64");
#elif PLATFORM_X86
			Logger.LogSystemInformation("AssetRipper Console Version", "x86");
#endif

			try
			{
				Ripper ripper = new Ripper();
				GameStructure gameStructure = ripper.Load(options.FilesToExport);
				PrepareExportDirectory(options.OutputDirectory.FullName);
				ripper.Export(options.OutputDirectory.FullName);
			}
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
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