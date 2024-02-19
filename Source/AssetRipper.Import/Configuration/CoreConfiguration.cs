using AssetRipper.Import.Logging;
using AssetRipper.Import.Utils;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using System.Diagnostics;

namespace AssetRipper.Import.Configuration
{
	public class CoreConfiguration
	{
		#region Import Settings
		/// <summary>
		/// Disabling scripts can allow some games to export when they previously did not.
		/// </summary>
		public bool DisableScriptImport => ScriptContentLevel == ScriptContentLevel.Level0;
		/// <summary>
		/// The level of scripts to export
		/// </summary>
		public ScriptContentLevel ScriptContentLevel { get; set; }
		/// <summary>
		/// Including the streaming assets directory can cause some games to fail while exporting.
		/// </summary>
		public bool IgnoreStreamingAssets
		{
			get => StreamingAssetsMode == StreamingAssetsMode.Ignore;
			set
			{
				StreamingAssetsMode = value ? StreamingAssetsMode.Ignore : StreamingAssetsMode.Extract;
			}
		}
		/// <summary>
		/// How the StreamingAssets folder is handled
		/// </summary>
		public StreamingAssetsMode StreamingAssetsMode { get; set; }
		/// <summary>
		/// The default version used when no version is specified, ie when the version has been stripped.
		/// </summary>
		public UnityVersion DefaultVersion { get; set; }
		#endregion

		#region Export Settings
		/// <summary>
		/// The root path to export to
		/// </summary>
		public string ExportRootPath { get; set; } = "";
		/// <summary>
		/// The path to create a new unity project in
		/// </summary>
		public string ProjectRootPath => Path.Combine(ExportRootPath, "ExportedProject");
		public string AssetsPath => Path.Combine(ProjectRootPath, "Assets");
		public string ProjectSettingsPath => Path.Combine(ProjectRootPath, "ProjectSettings");
		public string AuxiliaryFilesPath => Path.Combine(ExportRootPath, "AuxiliaryFiles");
		public BundledAssetsExportMode BundledAssetsExportMode { get; set; }
		#endregion

		#region Project Settings
		public UnityVersion Version { get; private set; }
		public BuildTarget Platform { get; private set; }
		public TransferInstructionFlags Flags { get; private set; }
		#endregion

		public SingletonDataStorage SingletonData { get; } = new();
		public ListDataStorage ListData { get; } = new();

		public CoreConfiguration()
		{
			ResetToDefaultValues();
			AddDebugData();
		}

		public void SetProjectSettings(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}

		public virtual void ResetToDefaultValues()
		{
			ScriptContentLevel = ScriptContentLevel.Level2;
			StreamingAssetsMode = StreamingAssetsMode.Extract;
			DefaultVersion = default;
			ExportRootPath = ExecutingDirectory.Combine("Ripped");
			BundledAssetsExportMode = BundledAssetsExportMode.DirectExport;
			SingletonData.Clear();
			ListData.Clear();
		}

		public virtual void LogConfigurationValues()
		{
			Logger.Info(LogCategory.General, $"Configuration Settings:");
			Logger.Info(LogCategory.General, $"{nameof(ScriptContentLevel)}: {ScriptContentLevel}");
			Logger.Info(LogCategory.General, $"{nameof(StreamingAssetsMode)}: {StreamingAssetsMode}");
			Logger.Info(LogCategory.General, $"{nameof(DefaultVersion)}: {DefaultVersion}");
			Logger.Info(LogCategory.General, $"{nameof(ExportRootPath)}: {ExportRootPath}");
			Logger.Info(LogCategory.General, $"{nameof(BundledAssetsExportMode)}: {BundledAssetsExportMode}");
		}

		[Conditional("DEBUG")]
		private void AddDebugData()
		{
			SingletonData.Add("README", "This is a singleton entry. It is used to store information that can be contained in a single file.");
			ListData.Add("README", ["This is a list entry. It is used to store information that might be contained in multiple files."]);
			ListData.Add("Fibonacci", [1, 1, 2, 3, 5, 8, 13, 21, 34, 55]);
			ListData.Add("Unused Key", []);
		}
	}
}
