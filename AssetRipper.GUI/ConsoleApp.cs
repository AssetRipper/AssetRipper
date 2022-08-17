using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using CommandLine;
using CommandLine.Text;

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

		internal class Options
		{
			[Option('i', "input", Required = true, HelpText = "Input files or directory to export.")]
			public IReadOnlyList<string> FilesToExport { get; set; }

			[Option('o', "output", HelpText = "Directory to export to. Will be cleared if already exists.")]
			public DirectoryInfo OutputDirectory { get; set; }

			[Option("logFile", HelpText = "(Default: " + DefaultLogFileName + ") File to log to.")]
			public FileInfo LogFile { get; set; }

			[Option('v', "verbose", Default = false, HelpText = "Verbose logging output.")]
			public bool Verbose { get; set; }

			[Option('q', "quit", Default = false, HelpText = "Close console after export.")]
			public bool Quit { get; set; }

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

		public static void ParseArgumentsAndRun(string[] args)
		{
			Parser parser = new Parser(with => with.HelpWriter = null);

			ParserResult<Options> parserResult = parser.ParseArguments<Options>(args);

			parserResult
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

				ripper.Settings.LogConfigurationValues();
				ripper.Load(options.FilesToExport);
				PrepareExportDirectory(options.OutputDirectory.FullName);
				ripper.ExportProject(options.OutputDirectory.FullName);
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
