﻿using AssetRipper.Export.UnityProjects;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Utils;
using AssetRipper.IO.Files.Streams.MultiFile;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.GUI
{
	internal static class ConsoleApp
	{
		private const string DefaultLogFileName = "AssetRipper.log";
		private static List<string>? FilesToExport { get; set; }
		private static DirectoryInfo? OutputDirectory { get; set; }
		private static FileInfo? LogFile { get; set; }
		private static bool Quit { get; set; }
		private static FileInfo? ConfigFile { get; set; }

		public static void ParseArgumentsAndRun(string[] args)
		{
			Parse(args);
			if (FilesToExport is null)
			{
				//This is only null if the user uses the --help or --version option.
				return;
			}
			else if (!ValidatePaths(FilesToExport))
			{
				Environment.ExitCode = 1;
			}
			else
			{
#if !DEBUG
				try
#endif
				{
					Logger.Add(new ConsoleLogger(false));
					if (LogFile?.Directory is not null)
					{
						Directory.CreateDirectory(LogFile.Directory.FullName);
					}
					Logger.Add(new FileLogger(LogFile?.FullName ?? ExecutingDirectory.Combine(DefaultLogFileName)));
					Run(FilesToExport, OutputDirectory?.FullName ?? ExecutingDirectory.Combine("Ripped"), ConfigFile);
				}
#if !DEBUG
				catch (Exception ex)
				{
					Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
				}
#endif
			}

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

			Option<FileInfo?> configFileOption = new Option<FileInfo?>(
							name: "--config-file",
							description: "Config file to use",
							getDefaultValue: () => null);
			rootCommand.AddOption(configFileOption);

			rootCommand.SetHandler((List<string> filesToExport, DirectoryInfo? outputDirectory, FileInfo? logFile, bool verbose, bool quit, FileInfo? configFile) =>
			{
				FilesToExport = filesToExport;
				OutputDirectory = outputDirectory;
				LogFile = logFile;
				Logger.AllowVerbose = verbose;
				Quit = quit;
				ConfigFile = configFile;
			},
			filesToExportOption, outputOption, logFileOption, verboseOption, quitOption, configFileOption);

			new CommandLineBuilder(rootCommand)
				.UseDefaults()
				.Build()
				.Invoke(args);
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

		private static void Run(IReadOnlyList<string> filesToExport, string outputDirectory, FileInfo? configFile)
		{
			Logger.LogSystemInformation("AssetRipper Console Version");

			Ripper ripper = new();
			if (configFile != null)
			{
				Logger.Info("Using config file: " + configFile.FullName);
				string config = File.ReadAllText(configFile.FullName);
				if (config == null)
				{
					Logger.Error("Failed to read config file");
					return;
				}
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					Converters = {
						new JsonStringEnumConverter(),
						new UnityVersionJsonConverter(),
					}
				};
				LibraryConfiguration? libraryConfiguration = JsonSerializer.Deserialize<LibraryConfiguration>(config, options);
				if (libraryConfiguration == null)
				{
					Logger.Error("Failed to deserialize config file");
					return;
				}
				ripper = new Ripper(libraryConfiguration);
			}

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
