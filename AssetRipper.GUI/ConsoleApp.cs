using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using AssetRipper.Library;
using CommandLine;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.GUI
{
	internal static class ConsoleApp
	{
		private const string DefaultLogFileName = "AssetRipper.log";

		internal class Options
		{
			[Value(0, Required = true, HelpText = "Input files or directory to export.")]
			public IReadOnlyList<string> FilesToExport { get; set; }

			[Option('o', "output", HelpText = "Directory to export to. Will be cleared if already exists.")]
			public DirectoryInfo OutputDirectory { get; set; }

			[Option("logFile", HelpText = "(Default: " + DefaultLogFileName + ") File to log to.")]
			public FileInfo LogFile { get; set; }

			[Option('v', "verbose", Default = false, HelpText = "Verbose logging output.")]
			public bool Verbose { get; set; }

			[Option('q', "quit", Default = false, HelpText = "Close console after export.")]
			public bool Quit { get; set; }

			[Option('f', "filespec", HelpText = "File pattern to export, including glob wildcards * ** ?")]
			public String FileSpec { get; set; }
		}

		public static void ParseArgumentsAndRun(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args)
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
				})
				.WithNotParsed((errors) =>
				{
					System.Console.ReadKey();
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
				options.LogFile ??= new FileInfo(ExecutingDirectory.Combine(DefaultLogFileName));
				options.OutputDirectory ??= new DirectoryInfo(ExecutingDirectory.Combine("Ripped"));
				options.FileSpec ??= "*";
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
			Logger.AllowVerbose = options.Verbose;
			Logger.Add(new ConsoleLogger(false));
			Logger.Add(new FileLogger(options.LogFile.FullName));
			Logger.LogSystemInformation("AssetRipper Console Version");
#if !DEBUG
			try
#endif
			{
				Ripper ripper = new();
				ripper.Settings.LogConfigurationValues();
				ripper.Load(options.FilesToExport);
				PrepareExportDirectory(options.OutputDirectory.FullName);
				ripper.ExportProject(options.OutputDirectory.FullName, options.FileSpec);
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
			if (Directory.Exists(path))
			{
				Logger.Info("Clearing export directory...");
				Directory.Delete(path, true);
			}
		}
	}
}
