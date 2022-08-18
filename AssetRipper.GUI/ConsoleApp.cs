using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.Builder;

using AssetRipper.Core.Configuration;
using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using AssetRipper.Library;
using AssetRipper.Library.Configuration;
using AssetRipper.Core.Classes.Misc;

namespace AssetRipper.GUI
{
	internal static class ConsoleApp
	{
		private const string DefaultLogFileName = "AssetRipper.log";
		private const string DefaultOutputPath = "Ripped";
		private const bool DefaultQuit = false;
		private const bool DefaultVerbose = false;

		private static void SetupLogger(bool verbose, string path)
		{
			Logger.AllowVerbose = verbose;
			Logger.Add(new ConsoleLogger(false));

			// Do not log to a file if the logging target is null. It should be AssetRipper.log
			// if the user didn't specify one.
			if (!string.IsNullOrEmpty(path))
				Logger.Add(new FileLogger(path));
		}

		private static void DoRipping(Ripper ripper, List<string> inputPaths, string outputPath)
		{
			Logger.LogSystemInformation("AssetRipper Console Version");
			ripper.Settings.LogConfigurationValues();

			ripper.Load(inputPaths);

			PrepareExportDirectory(outputPath);
			ripper.ExportProject(outputPath);
		}

		private static Task RootCommandHandler(InvocationContext context, Ripper ripper)
		{
			List<string>? inputs = context.ParseResult.GetValueForArgument(inputArgument);
			DirectoryInfo? output = context.ParseResult.GetValueForOption(outputOption);
			bool quit = context.ParseResult.GetValueForOption(quitOption);

			if (inputs == null)
				throw new Exception("Internal error - Input was lost");

			if (output == null)
				throw new Exception("Internal error - Output was lost");

			SetupLogger(false, DefaultLogFileName);

			try
			{
				DoRipping(ripper, inputs, output.FullName);
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
			finally
			{
				if (!quit)
				{
					Console.WriteLine($"AssetRipper finished, press any key to exit. Pass -q to quit automatically.");
					Console.ReadKey();
				}
			}

			return Task.CompletedTask;
		}

		private static void ExtractCommandHandler(InvocationContext context, Ripper ripper, Dictionary<string, Option> options)
		{
			List<string>? inputs = context.ParseResult.GetValueForOption(inputOption);
			DirectoryInfo? output = context.ParseResult.GetValueForOption(outputOption);
			FileInfo? logFile = context.ParseResult.GetValueForOption(logFileOption);

			bool verbose = context.ParseResult.GetValueForOption(verboseOption);

			// Get a list of options supported by LibraryConfiguration
			// MUST be run after defaults have been filled, as it only lists
			// options with values.
			foreach (Type type in ripper.Settings.settings.Keys.ToList())
			{
				string optionName = $"-{ToHyphenCase(type.Name)}";

				if (type == null)
					continue;

				options.TryGetValue(optionName, out Option? option);

				if (option == null)
					continue;

				object? settingValue = context.ParseResult.GetValueForOption(option);

				if (settingValue == null)
					continue;

				string? enumKey = settingValue.ToString();

				if (string.IsNullOrEmpty(enumKey))
					continue;

				ripper.Settings.SetSetting(type, Enum.Parse(type, enumKey));
			}

			if (inputs == null)
				throw new Exception("Internal error - Input was lost");

			if (output == null)
				throw new Exception("Internal error - Output was lost");

			if (logFile != null)
				SetupLogger(verbose, logFile.FullName);

			DoRipping(ripper, inputs, output.FullName);
		}

		#region Options

		private static Argument<List<string>>? _inputArgument;

		private static Argument<List<string>> inputArgument
		{
			get
			{
				if (_inputArgument != null)
					return _inputArgument;

				Argument<List<string>> argument = new Argument<List<string>>(name: "input", description: "Input files or directory to export");

				argument.AddValidator((symbolResult) =>
				{
					List<string>? inputs = symbolResult.GetValueForArgument(argument);

					if (inputs.Count < 1)
					{
						symbolResult.ErrorMessage = "No files to export. You must provide at least one path using --input. Use --help for help.";
						return;
					}

					foreach (string input in inputs)
					{
						string path = ExecutingDirectory.Combine(input);

						if (!MultiFileStream.Exists(path) && !Directory.Exists(path))
						{
							symbolResult.ErrorMessage = MultiFileStream.IsMultiFile(path)
								? $"File '{path}' doesn't have all parts for combining"
								: $"Neither file nor directory with path '{path}' exists";
							return;
						}
					}
				});

				_inputArgument = argument;

				return argument;
			}
		}

		private static Option<List<string>>? _inputOption;

		private static Option<List<string>> inputOption
		{
			get {
				if (_inputOption != null)
					return _inputOption;

				Option<List<string>> option = new Option<List<string>>(name: "--input", description: "Input files or directory to export");
				option.AddAlias("-i");
				option.IsRequired = true;

				option.AddValidator((symbolResult) =>
				{
					List<string>? inputs = symbolResult.GetValueForOption(option);

					if (inputs == null || inputs.Count < 1)
					{
						symbolResult.ErrorMessage = "No files to export. You must provide at least one path using --input. Use --help for help.";
						return;
					}

					foreach (string input in inputs)
					{
						string path = ExecutingDirectory.Combine(input);

						if (!MultiFileStream.Exists(path) && !Directory.Exists(path))
						{
							symbolResult.ErrorMessage = MultiFileStream.IsMultiFile(path)
								? $"File '{path}' doesn't have all parts for combining"
								: $"Neither file nor directory with path '{path}' exists";
							return;
						}
					}
				});

				_inputOption = option;

				return option;
			}
		}

		private static Option<DirectoryInfo>? _outputOption;

		private static Option<DirectoryInfo> outputOption
		{
			get
			{
				if (_outputOption != null)
					return _outputOption;

				Option<DirectoryInfo> option = new Option<DirectoryInfo>(name: "--output", description: "Directory to export to (will be cleared if already exists)");
				option.AddAlias("-o");
				option.SetDefaultValue(DefaultOutputPath);
				option.IsRequired = false;

				option.AddValidator((symbolResult) =>
				{
					DirectoryInfo? dirInfo = symbolResult.GetValueForOption(option);

					if (dirInfo == null)
						dirInfo = new DirectoryInfo(DefaultOutputPath);

					string path = ExecutingDirectory.Combine(dirInfo.FullName);

					if (!Directory.Exists(System.IO.Path.GetDirectoryName(path)))
						symbolResult.ErrorMessage = "Directory for output does not exist";
				});

				_outputOption = option;

				return option;
			}
		}

		private static Option<bool>? _quitOption;

		private static Option<bool> quitOption
		{
			get
			{
				if (_quitOption != null)
					return _quitOption;

				Option<bool> option = new Option<bool>(name: "--quit", description: "Quit after ripping");
				option.AddAlias("-q");
				option.SetDefaultValue(DefaultQuit);

				_quitOption = option;

				return option;
			}
		}

		private static Option<bool>? _verboseOption;

		private static Option<bool> verboseOption
		{
			get
			{
				if (_verboseOption != null)
					return verboseOption;

				Option<bool> option = new Option<bool>(name: "--verbose", description: "Verbose logging output");
				option.AddAlias("-v");
				option.SetDefaultValue(DefaultVerbose);

				_verboseOption = option;

				return option;
			}
		}

		private static Option<FileInfo>? _logFileOption;

		private static Option<FileInfo> logFileOption
		{
			get
			{
				if (_logFileOption != null)
					return _logFileOption;

				Option<FileInfo> option = new Option<FileInfo>(name: "--log-file", description: "File to log to");
				option.AddAlias("-w");
				option.SetDefaultValue(DefaultLogFileName);

				option.AddValidator((symbolResult) =>
				{
					FileInfo? fileInfo = symbolResult.GetValueForOption(option);

					if (fileInfo == null)
						fileInfo = new FileInfo(DefaultLogFileName);

					string path = ExecutingDirectory.Combine(fileInfo.FullName);

					if (!Directory.Exists(System.IO.Path.GetDirectoryName(path)))
						symbolResult.ErrorMessage = "Directory for log file does not exist";
				});

				_logFileOption = option;

				return option;
			}
		}

		#endregion

		private static string ToHyphenCase(string input)
		{
			IEnumerable<string> result = input.Select(x =>
			 {
				 if (char.IsUpper(x)) return "-" + char.ToLower(x);
				 return x.ToString();
			 });

			return string.Join("", result);
		}

		private static Dictionary<string, Option> CreateOptions(Ripper ripper)
		{
			Dictionary<string, Option> options = new Dictionary<string, Option>();

			foreach (Type type in ripper.Settings.settings.Keys)
			{
				string optionName = $"-{ToHyphenCase(type.Name)}";
				Option option = new Option<string>(name: optionName);

				if (type.IsEnum)
				{
					string[] values = Enum.GetNames(type);
					option.AddCompletions(values);

					option.AddValidator((symbolResult) =>
					{
						object? input = symbolResult.GetValueForOption(option);

						if (input == null)
							return;

						string[] names = Enum.GetNames(type);

						if (!names.Contains(input))
						{
							symbolResult.ErrorMessage = $"{input} is not a valid option. Valid options: {string.Join(", ", names)}";
							return;
						}
					});
				}

				options.Add(optionName, option);
			}

			return options;
		}

		private static void HandleException(Exception ex)
		{
#if DEBUG
			Console.WriteLine("===================================================");
			Console.WriteLine("Error during command invocation, extraction aborted");
			Console.WriteLine("===================================================");
			Console.WriteLine(ex.ToString());
#else
			Console.WriteLine("Error during command invocation, extraction aborted");
			Console.WriteLine("See this message below for details:");
			Console.WriteLine(ex.Message);
#endif
		}

		public static void ParseArgumentsAndRun(string[] args)
		{
			Ripper ripper = new();
			ripper.Settings.ResetToDefaultValues();

			Dictionary<string, Option> options = CreateOptions(ripper);

			RootCommand rootCommand = new RootCommand();
			rootCommand.SetHandler((InvocationContext context) => RootCommandHandler(context, ripper));
			rootCommand.AddArgument(inputArgument);
			rootCommand.AddOption(outputOption);
			rootCommand.AddOption(quitOption);

			Command extractCommand = new Command("extract");
			extractCommand.SetHandler((InvocationContext context) => ExtractCommandHandler(context, ripper, options));
			extractCommand.AddAlias("e");
			extractCommand.AddOption(inputOption);
			extractCommand.AddOption(outputOption);
			extractCommand.AddOption(verboseOption);
			extractCommand.AddOption(logFileOption);

			foreach (Option option in options.Values)
			{
				extractCommand.AddOption(option);
			}

			rootCommand.AddCommand(extractCommand);

			Parser parser = new CommandLineBuilder(rootCommand)
				.UseVersionOption()
				.UseHelp()
				.RegisterWithDotnetSuggest()
				.UseTypoCorrections()
				.UseParseErrorReporting()
				.CancelOnProcessTermination()
				.Build();

			try
			{
				parser.Parse(args).Invoke();
			} catch (Exception ex)
			{
				HandleException(ex);
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
