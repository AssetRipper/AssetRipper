using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;

using System.CommandLine;
using System.CommandLine.Invocation;

using AssetRipper.Core.Configuration;
using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using AssetRipper.Library;
using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI
{
	internal static class ConsoleApp
	{
		private const string DefaultLogFileName = "AssetRipper.log";
		private const string DefaultOutputPath = ".";

		/*
		internal class Options
		{
			[Option('k', "keep-open", Default = false, HelpText = "Keep console open after exporting finishes")]
			public bool KeepOpen { get; set; }

			[Option('b', "bundle-mode", Default = BundledAssetsExportMode.GroupByBundleName, HelpText = "Bundled asset export mode\n")]
			public BundledAssetsExportMode BundledAssetsExportMode { get; set; }

			[Option('g', "ignore-streaming", Default = false, HelpText = "Ignore StreamingAssets folder")]
			public bool IgnoreStreaming { get; set; }

			[Option('r', "script-level", Default = ScriptContentLevel.Level2, HelpText = "Script content level\n")]
			public ScriptContentLevel ScriptContentLevel { get; set; }
		}
		*/

		private static void RootCommandHandler(InvocationContext context, Ripper ripper, Dictionary<string, Option> options)
		{
			// TODO: Remove this
			Console.WriteLine("Root command handler");

			// TODO: Don't run the extract handler, but refactor that so that extracting happens elsewhere.
			// Then call that from here with -k, as the root command is called when a file is dropped onto the exe.
			ExtractCommandHandler(context, ripper, options);
		}

		private static void ExtractCommandHandler(InvocationContext context, Ripper ripper, Dictionary<string, Option> options)
		{
			List<string>? input = context.ParseResult.GetValueForOption(inputOption);
			DirectoryInfo? output = context.ParseResult.GetValueForOption(outputOption);
			FileInfo? logFile = context.ParseResult.GetValueForOption(logFileOption);
			bool verbose = context.ParseResult.GetValueForOption(verboseOption);

			Logger.AllowVerbose = verbose;
			Logger.Add(new ConsoleLogger(false));

			// Do not log to a file if the logging target is null. It should be AssetRipper.log
			// if the user didn't specify one.
			if (logFile != null)
				Logger.Add(new FileLogger(logFile.FullName));

			Logger.LogSystemInformation("AssetRipper Console Version");

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

			ripper.Settings.LogConfigurationValues();

			// FilesToExport and OutputDirectory shouldn't be null here, as CommandLine should have set them,
			// but technically they could be null as their nullable type suggests.
			if (input == null)
				throw new Exception("Internal error - list of files to export was lost");
			else
				ripper.Load(input);

			if (output == null)
				throw new Exception("Internal error - output directory was lost");
			else
			{
				PrepareExportDirectory(output.FullName);
				ripper.ExportProject(output.FullName);
			}
		}

		private static Option<List<string>> inputOption
		{
			get {
				Option<List<string>> option = new Option<List<string>>(name: "--input", description: "Input files or directory to export");
				option.AddAlias("-i");
				option.IsRequired = true;

				option.AddValidator((symbolResult) =>
				{
					List<string>? inputs = symbolResult.GetValueForOption(option);

					if (inputs == null)
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

				return option;
			}
		}

		private static Option<DirectoryInfo> outputOption
		{
			get
			{
				Option<DirectoryInfo> option = new Option<DirectoryInfo>(name: "--output", description: "Directory to export to (will be cleared if already exists)");
				option.AddAlias("-o");
				option.SetDefaultValue(".");
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

				return option;
			}
		}

		private static Option<bool> verboseOption
		{
			get
			{
				Option<bool> option = new Option<bool>(name: "--verbose", description: "Verbose logging output");
				option.AddAlias("-v");
				option.SetDefaultValue(false);

				return option;
			}
		}

		private static Option<FileInfo> logFileOption
		{
			get
			{
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

				return option;
			}
		}

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

			options.Add(inputOption.Name, inputOption);
			options.Add(outputOption.Name, outputOption);
			options.Add(verboseOption.Name, verboseOption);
			options.Add(logFileOption.Name, logFileOption);

			foreach (Type type in ripper.Settings.settings.Keys)
			{
				string optionName = $"-{ToHyphenCase(type.Name)}";
				Option option = new Option<string>(name: optionName);

				if (type.IsEnum)
				{
					string[] values = Enum.GetNames(type);
					option.AddCompletions(values);
				}

				options.Add(optionName, option);
			}

			return options;
		}

		public static void ParseArgumentsAndRun(string[] args)
		{
			Ripper ripper = new();
			ripper.Settings.ResetToDefaultValues();

			Dictionary<string, Option> options = CreateOptions(ripper);

			RootCommand rootCommand = new RootCommand("AssetRipper");
			rootCommand.SetHandler((InvocationContext context) => RootCommandHandler(context, ripper, options));

			Command extractCommand = new Command("extract");
			extractCommand.AddAlias("e");
			extractCommand.SetHandler((InvocationContext context) => ExtractCommandHandler(context, ripper, options));

			foreach (Option option in options.Values)
			{
				extractCommand.AddOption(option);
			}

			rootCommand.AddCommand(extractCommand);
			rootCommand.Invoke(args);

			/*
			Parser parser = new Parser(with => with.HelpWriter = null);

			ParserResult<Options> parserResult = parser.ParseArguments<Options>(args);

			parserResult
				.WithParsed(options =>
				{
					if (ValidateOptions(options))
						Run(options);
					else
						Environment.ExitCode = 1;

					if (options.KeepOpen)
					{
						Console.WriteLine("AssetRipper finished, press any key to exit.");
						Console.ReadKey();
					}
				})
				.WithNotParsed((errors) =>
				{
					HelpText? helpText = HelpText.AutoBuild(parserResult, h =>
					{
						Process currentProcess = Process.GetCurrentProcess();

						// System.Diagnostics provides nullable interfaces, so we check all of them
						// and skip showing the usage example if any of them are null.
						if (
							currentProcess != null
							&& currentProcess.MainModule != null
							&& !string.IsNullOrEmpty(currentProcess.MainModule.FileName)
						)
						{
							string exeName = Path.GetFileName(currentProcess.MainModule.FileName);
							h.AddPreOptionsText($"Example: {exeName} -i Games/MyGame -o Code/MyGameDecompiled");
						}

#if DEBUG
						h.AddPreOptionsText(string.Join('\n', errors));
#endif

						h.AddEnumValuesToHelpText = true;
						h.AdditionalNewLineAfterOption = true;
						return h;
					}, e => e);

					Console.WriteLine(helpText);
				});
			*/
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
