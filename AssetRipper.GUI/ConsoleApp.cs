using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using AssetRipper.Library;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;

namespace AssetRipper.GUI
{
	internal static class ConsoleApp
	{
		private const string DefaultLogFileName = "AssetRipper.log";
		private static bool Quit { get; set; }

		public static void ParseArgumentsAndRun(string[] args)
		{
			Logger.Add(new ConsoleLogger(false));
#if !DEBUG
			try
#endif
			{
				Parse(args);
			}
#if !DEBUG
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
#endif
			if (!Quit)
			{
				Console.ReadKey();
			}
		}

		private static void Parse(string[] args)
		{
			RootCommand rootCommand = new() { Description = "AssetRipper Console" };

			Argument<List<string>> filesToExportOption = new();
			rootCommand.AddArgument(filesToExportOption);

			Option<DirectoryInfo?> outputOption = new Option<DirectoryInfo?>(
							aliases: new[] { "-o", "--output" },
							description: "",
							getDefaultValue: () => null);
			rootCommand.AddOption(outputOption);

			Option<FileInfo?> logFileOption = new Option<FileInfo?>(
							name: "--log-file",
							description: "",
							getDefaultValue: () => null);
			rootCommand.AddOption(logFileOption);

			Option<bool> verboseOption = new Option<bool>(
							aliases: new[] { "-v", "--verbose" },
							description: "",
							getDefaultValue: () => false);
			rootCommand.AddOption(verboseOption);

			Option<bool> quitOption = new Option<bool>(
							aliases: new[] { "-q", "--quit" },
							description: "",
							getDefaultValue: () => false);
			rootCommand.AddOption(quitOption);

			rootCommand.SetHandler((List<string> filesToExport, DirectoryInfo? outputDirectory, FileInfo? logFile, bool verbose, bool quit) =>
			{
				Quit = quit;
				if (!ValidatePaths(filesToExport))
				{
					Environment.ExitCode = 1;
				}
				else
				{
					Logger.AllowVerbose = verbose;
					Logger.Add(new FileLogger(logFile?.FullName ?? ExecutingDirectory.Combine(DefaultLogFileName)));
					Run(filesToExport, outputDirectory?.FullName ?? ExecutingDirectory.Combine("Ripped"));
				}
			},
			filesToExportOption, outputOption, logFileOption, verboseOption, quitOption);

			rootCommand.Invoke(args);
		}

		private static bool ValidatePaths(IReadOnlyList<string> paths)
		{
			foreach (string path in paths)
			{
				if (!MultiFileStream.Exists(path) && !Directory.Exists(path))
				{
					Console.WriteLine(MultiFileStream.IsMultiFile(path)
						? $"File '{path}' doesn't have all parts for combining"
						: $"Neither file nor directory with path '{path}' exists");

					return false;
				}
			}
			return true;
		}

		private static void Run(IReadOnlyList<string> filesToExport, string outputDirectory)
		{
			Logger.LogSystemInformation("AssetRipper Console Version");

			Ripper ripper = new();
			ripper.Settings.LogConfigurationValues();
			ripper.Load(filesToExport);
			PrepareExportDirectory(outputDirectory);
			ripper.ExportProject(outputDirectory);
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
