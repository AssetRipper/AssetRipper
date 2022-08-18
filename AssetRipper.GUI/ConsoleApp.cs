using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.CommandLine.Builder;

using AssetRipper.Core.Logging;
using AssetRipper.Library;

namespace AssetRipper.GUI
{
	internal static class ConsoleApp
	{
		private static void SetupLogger(bool verbose, string path)
		{
			Logger.AllowVerbose = verbose;
			Logger.Add(new ConsoleLogger(false));

			// Do not log to a file if the logging target is null. It should be AssetRipper.log
			// if the user didn't specify one.
			if (string.IsNullOrEmpty(path))
				return;

			Logger.Add(new FileLogger(path));
		}

		private static void DoRipping(Ripper ripper, List<string> inputPaths, string outputPath)
		{
			Logger.LogSystemInformation("AssetRipper Console Version");
			ripper.Settings.LogConfigurationValues();
			ripper.Load(inputPaths);

			if (Directory.Exists(outputPath))
			{
				Logger.Info("Clearing export directory...");
				Directory.Delete(outputPath, true);
			}

			ripper.ExportProject(outputPath);
		}

		private static void RootCommandHandler(InvocationContext context, Ripper ripper)
		{
			ConsoleOptions.SetContext(context);

			List<string> inputs = ConsoleOptions.GetArgumentOrFallback(ConsoleOptions.inputArgument, new List<string>());
			DirectoryInfo output = ConsoleOptions.GetOptionOrFallback(ConsoleOptions.outputOption, new DirectoryInfo(ConsoleOptions.DefaultOutputPath));
			bool quit = ConsoleOptions.GetOptionOrFallback(ConsoleOptions.quitOption, false);

			if (inputs == null)
				throw new Exception("Internal error - Input was lost");

			if (output == null)
				throw new Exception("Internal error - Output was lost");

			SetupLogger(false, ConsoleOptions.DefaultLogFileName);

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
		}

		private static void ExtractCommandHandler(InvocationContext context, Ripper ripper, Dictionary<string, Option> options)
		{
			List<string>? inputs = ConsoleOptions.GetOptionOrFallback(ConsoleOptions.inputOption, new List<string>());
			DirectoryInfo? output = ConsoleOptions.GetOptionOrFallback(ConsoleOptions.outputOption, new DirectoryInfo(ConsoleOptions.DefaultOutputPath));
			FileInfo? logFile = ConsoleOptions.GetOptionOrFallback(ConsoleOptions.logFileOption, new FileInfo(ConsoleOptions.DefaultLogFileName));
			bool verbose = ConsoleOptions.GetOptionOrFallback(ConsoleOptions.verboseOption, false);

			// Get a list of options supported by LibraryConfiguration
			// MUST be run after defaults have been filled, as it only lists
			// options with values.
			foreach (Type type in ripper.Settings.settings.Keys.ToList())
			{
				string optionName = ConsoleOptions.OptionNameFromType(type);

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

		private static void HandleException(Exception ex)
		{
			Environment.ExitCode = 1;

#if DEBUG
			Console.WriteLine("===================================================");
			Console.WriteLine("      Error during ripping, extraction aborted     ");
			Console.WriteLine("===================================================");
			Console.WriteLine(ex.ToString());
#else
			Console.WriteLine("Error during ripping, extraction aborted");
			Console.WriteLine("See this message below for details:");
			Console.WriteLine(ex.Message);
#endif
		}

		public static void ParseArgumentsAndRun(string[] args)
		{
			Ripper ripper = new();
			ripper.Settings.ResetToDefaultValues();

			Dictionary<string, Option> options = ConsoleOptions.GenerateFromRipper(ripper);

			RootCommand rootCommand = new RootCommand();
			rootCommand.SetHandler((InvocationContext context) => RootCommandHandler(context, ripper));
			rootCommand.AddArgument(ConsoleOptions.inputArgument);
			rootCommand.AddOption(ConsoleOptions.outputOption);
			rootCommand.AddOption(ConsoleOptions.quitOption);

			Command extractCommand = new Command("extract");
			extractCommand.SetHandler((InvocationContext context) => ExtractCommandHandler(context, ripper, options));
			extractCommand.AddAlias("e");
			extractCommand.AddOption(ConsoleOptions.inputOption);
			extractCommand.AddOption(ConsoleOptions.outputOption);
			extractCommand.AddOption(ConsoleOptions.verboseOption);
			extractCommand.AddOption(ConsoleOptions.logFileOption);

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
	}
}
