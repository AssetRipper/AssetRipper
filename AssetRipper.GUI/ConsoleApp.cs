using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

using System.CommandLine;
using System.CommandLine.Invocation;

using AssetRipper.Core.Configuration;
using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using AssetRipper.Library;
using AssetRipper.Library.Configuration;
using YamlDotNet.Core.Tokens;

namespace AssetRipper.GUI
{
	internal static class ConsoleApp
	{
		private const string DefaultLogFileName = "AssetRipper.log";

		/*
		internal class Options
		{
			[Option('i', "input", Required = true, HelpText = "Input files or directory to export")]
			public IReadOnlyList<string>? FilesToExport { get; set; }

			[Option('o', "output", Required = true, HelpText = "Directory to export to (will be cleared if already exists)")]
			public DirectoryInfo? OutputDirectory { get; set; }

			[Option('w', "log-file", Default = DefaultLogFileName, HelpText = "File to log to")]
			public FileInfo? LogFile { get; set; }

			[Option('v', "verbose", Default = false, HelpText = "Verbose logging output")]
			public bool Verbose { get; set; }

			[Option('k', "keep-open", Default = false, HelpText = "Keep console open after exporting finishes")]
			public bool KeepOpen { get; set; }

			[Option('b', "bundle-mode", Default = BundledAssetsExportMode.GroupByBundleName, HelpText = "Bundled asset export mode\n")]
			public BundledAssetsExportMode BundledAssetsExportMode { get; set; }

			[Option('g', "ignore-streaming", Default = false, HelpText = "Ignore StreamingAssets folder")]
			public bool IgnoreStreaming { get; set; }

			[Option('a', "audio-format", Default = AudioExportFormat.Default, HelpText = "Audio export format\n")]
			public AudioExportFormat AudioExportFormat { get; set; }

			[Option('p', "image-format", Default = ImageExportFormat.Png, HelpText = "Image export format\n")]
			public ImageExportFormat ImageExportFormat { get; set; }

			[Option('m', "mesh-format", Default = MeshExportFormat.Native, HelpText = "Mesh export format\n")]
			public MeshExportFormat MeshExportFormat { get; set; }

			[Option('s', "sprite-mode", Default = SpriteExportMode.Native, HelpText = "Sprite export mode\n")]
			public SpriteExportMode SpriteExportMode { get; set; }

			[Option('t', "terrain-mode", Default = TerrainExportMode.Yaml, HelpText = "Terrain export mode\n")]
			public TerrainExportMode TerrainExportMode { get; set; }

			[Option('x', "text-mode", Default = TextExportMode.Parse, HelpText = "Text export mode\n")]
			public TextExportMode TextExportMode { get; set; }

			[Option('d', "shader-mode", Default = ShaderExportMode.Dummy, HelpText = "Shader export mode\n")]
			public ShaderExportMode ShaderExportMode { get; set; }

			[Option('c', "script-mode", Default = ScriptExportMode.Hybrid, HelpText = "Script export mode\n")]
			public ScriptExportMode ScriptExportMode { get; set; }

			[Option('r', "script-level", Default = ScriptContentLevel.Level2, HelpText = "Script content level\n")]
			public ScriptContentLevel ScriptContentLevel { get; set; }

			[Option('l', "script-language-version", Default = ScriptLanguageVersion.AutoSafe, HelpText = "Script language version\n")]
			public ScriptLanguageVersion ScriptLanguageVersion { get; set; }
		}
		*/

		private static void RootCommandHandler(InvocationContext context, Ripper ripper)
		{
			// TODO: Remove this
			Console.WriteLine("Root command handler");

			// TODO: Don't run the extract handler, but refactor that so that extracting happens elsewhere.
			// Then call that from here with -k, as the root command is called when a file is dropped onto the exe.
			ExtractCommandHandler(context, ripper);
		}

		private static void ExtractCommandHandler(InvocationContext context, Ripper ripper)
		{
			// TODO: Remove this
			Console.WriteLine("Extract command handler");

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

			/*
			ripper.Settings.BundledAssetsExportMode = options.BundledAssetsExportMode;
			ripper.Settings.IgnoreStreamingAssets = options.IgnoreStreaming;
			ripper.Settings.AudioExportFormat = options.AudioExportFormat;
			ripper.Settings.ImageExportFormat = options.ImageExportFormat;
			ripper.Settings.MeshExportFormat = options.MeshExportFormat;
			ripper.Settings.SpriteExportMode = options.SpriteExportMode;
			ripper.Settings.TerrainExportMode = options.TerrainExportMode;
			ripper.Settings.TextExportMode = options.TextExportMode;
			ripper.Settings.ShaderExportMode = options.ShaderExportMode;
			ripper.Settings.ScriptExportMode = options.ScriptExportMode;
			ripper.Settings.ScriptContentLevel = options.ScriptContentLevel;
			ripper.Settings.ScriptLanguageVersion = options.ScriptLanguageVersion;
			*/

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

				return option;
			}
		}

		private static Option<DirectoryInfo> outputOption
		{
			get
			{
				Option<DirectoryInfo> option = new Option<DirectoryInfo>(name: "--output", description: "Directory to export to (will be cleared if already exists)");
				option.AddAlias("-o");

				return option;
			}
		}

		private static Option<bool> verboseOption
		{
			get
			{
				Option<bool> option = new Option<bool>(name: "--verbose", description: "Verbose logging output");
				option.AddAlias("-v");

				return option;
			}
		}

		private static Option<FileInfo> logFileOption
		{
			get
			{
				Option<FileInfo> option = new Option<FileInfo>(name: "--log-file", description: "File to log to");
				option.AddAlias("-w");

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

		private static List<Option> CreateOptions(Ripper ripper)
		{
			List<Option> list = new List<Option>();

			list.Add(inputOption);
			list.Add(outputOption);
			list.Add(verboseOption);
			list.Add(logFileOption);

			// WIP
			// Generate a list of options by looking at properties of the LibraryConfiguration class

			IEnumerable<object> values1 = typeof(LibraryConfiguration).GetFields(BindingFlags.Public | BindingFlags.Static).Select(a => (LibraryConfiguration)a.GetValue(null));

			Console.WriteLine($"Got {values1.Count()} values");

			foreach (object value in values1)
			{
				Console.WriteLine("Vaklue", value);
			}

			var members = typeof(LibraryConfiguration).GetMembers();
			Console.WriteLine($"[TRACE] Found {members.Length} members.");

			foreach (var member in members)
			{
				if (member.MemberType != MemberTypes.Property || member.DeclaringType != typeof(LibraryConfiguration))
					continue;

				Option option = new Option<int>(name: $"-{ToHyphenCase(member.Name)}");

				Console.WriteLine($"{member.Name} is enum: {member.GetType().IsEnum}. It is: {member.GetType()}. Member type: {member.MemberType}");

				if (member.GetType().IsEnum)
				{
					string[] values = Enum.GetNames(member.GetType());
					option.AddCompletions(values);
				}

				list.Add(option);
			}

			return list;
		}

		public static void ParseArgumentsAndRun(string[] args)
		{
			Ripper ripper = new();

			RootCommand rootCommand = new RootCommand("AssetRipper");
			rootCommand.SetHandler((InvocationContext context) => RootCommandHandler(context, ripper));

			Command extractCommand = new Command("extract");
			extractCommand.SetHandler((InvocationContext context) => ExtractCommandHandler(context, ripper));

			List<Option> options = CreateOptions(ripper);

			foreach (Option option in options)
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

		/* private static bool ValidateOptions(Options options)
		{
			if (options.FilesToExport == null)
			{
				Console.WriteLine("Found no files to export. Please specify at least one file or folder as input. Use --help for help.");
				return false;
			}

			if (options.OutputDirectory == null)
			{
				Console.WriteLine("No output directory is specified. Please spectfy the output directory. Use --help for help.");
				return false;
			}

			foreach (string arg in options.FilesToExport)
			{
				if (MultiFileStream.Exists(arg) || Directory.Exists(arg))
					continue;

				Console.WriteLine(MultiFileStream.IsMultiFile(arg)
					? $"File '{arg}' doesn't have all parts for combining"
					: $"Neither file nor directory with path '{arg}' exists");

				return false;
			}

			try
			{
				options.LogFile ??= new FileInfo(ExecutingDirectory.Combine(DefaultLogFileName));
				options.OutputDirectory ??= new DirectoryInfo(ExecutingDirectory.Combine("Ripped"));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to initialize the output and log paths.");
				Console.WriteLine(ex.ToString());
				return false;
			}

			return true;
		} */

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
