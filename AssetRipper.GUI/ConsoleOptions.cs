using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.CommandLine;
using System.CommandLine.Invocation;

using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Utils;
using AssetRipper.Library;


namespace AssetRipper.GUI
{
	internal static class ConsoleOptions
	{
		public const string DefaultLogFileName = "AssetRipper.log";
		public const string DefaultOutputPath = "Ripped";
		public const bool DefaultQuit = false;
		public const bool DefaultVerbose = false;

		private static InvocationContext? _context;

		#region Utilities

		public static string OptionNameFromType(Type input)
		{
			IEnumerable<string> result = input.Name.Select(x =>
			{
				if (char.IsUpper(x)) return "-" + char.ToLower(x);
				return x.ToString();
			});

			return $"-{string.Join("", result)}";
		}

		public static Dictionary<string, Option> GenerateFromRipper(Ripper ripper)
		{
			Dictionary<string, Option> options = new Dictionary<string, Option>();

			foreach (Type type in ripper.Settings.settings.Keys)
			{
				string optionName = OptionNameFromType(type);
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

		public static void SetContext(InvocationContext context)
		{
			_context = context;
		}

		public static T GetOptionOrFallback<T>(Option<T> option, T fallback)
		{
			if (_context == null)
			{
#if DEBUG
				Console.WriteLine("Programming error: Context was not set before trying to get an option from it. Returning fallback.");
#endif
				return fallback;
			}

			T? result = _context.ParseResult.GetValueForOption<T>(option);

			if (result == null)
				return fallback;

			return result;
		}

		public static T GetArgumentOrFallback<T>(Argument<T> argument, T fallback)
		{
			if (_context == null)
			{
#if DEBUG
				Console.WriteLine("Programming error: Context was not set before trying to get an argument from it. Returning fallback.");
#endif
				return fallback;
			}

			T? result = _context.ParseResult.GetValueForArgument<T>(argument);

			if (result == null)
				return fallback;

			return result;
		}

		#endregion

		#region Options and Arguments

		private static Argument<List<string>>? _inputArgument;

		public static Argument<List<string>> inputArgument
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

		public static Option<List<string>> inputOption
		{
			get
			{
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

		public static Option<DirectoryInfo> outputOption
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

		public static Option<bool> quitOption
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

		public static Option<bool> verboseOption
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

		public static Option<FileInfo> logFileOption
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
	}
}
