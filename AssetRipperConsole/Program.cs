using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using AssetRipper.Library;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Console
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

			[Option('v', "verbose", Default = false, HelpText = "Verbose logging output.")]
			public bool verbose { get; set; }

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
						System.Console.ReadKey();
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

				if (Directory.Exists(arg))
				{
					continue;
				}

				System.Console.WriteLine(MultiFileStream.IsMultiFile(arg)
					? $"File '{arg}' doesn't have all parts for combining"
					: $"Neither file nor directory with path '{arg}' exists");

				return false;
			}

			try
			{
				if (options.LogFile == null)
					options.LogFile = new FileInfo(ExecutingDirectory.Combine(DefaultLogFileName));
				if (options.OutputDirectory == null)
					options.OutputDirectory = new DirectoryInfo(ExecutingDirectory.Combine("Ripped"));
			}
			catch (Exception ex)
			{
				System.Console.WriteLine($"Failed to initialize the output and log paths.");
				System.Console.WriteLine(ex.ToString());
				return false;
			}

			return true;
		}

		private static void Run(Options options)
		{
			Logger.AllowVerbose = options.verbose;
			Logger.Add(new ConsoleLogger(false));
			Logger.Add(new FileLogger(options.LogFile.FullName));
			Logger.LogSystemInformation("AssetRipper Console Version");
			Core.Updating.UpdateManager.LogUpdateCheck();

			try
			{
				Ripper ripper = new Ripper();
				ripper.Settings.LogConfigurationValues();
				ripper.Load(options.FilesToExport);
				PrepareExportDirectory(options.OutputDirectory.FullName);
				ripper.ExportProject(options.OutputDirectory.FullName);
			}
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
		}

		private static void PrepareExportDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				Logger.Info("Clearing export directory...");
				Directory.Delete(path, true);
			}
		}
	}
}