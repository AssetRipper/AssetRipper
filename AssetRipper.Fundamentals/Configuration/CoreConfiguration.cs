using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Utils;
using System.IO;

namespace AssetRipper.Core.Configuration
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
		/// <summary>
		/// Should objects get exported with dependencies or without?
		/// </summary>
		public bool ExportDependencies { get; set; }
		public BundledAssetsExportMode BundledAssetsExportMode { get; set; }
		/// <summary>
		/// A function to determine if an object is allowed to be exported.<br/>
		/// Set by default to allow everything.
		/// </summary>
		public Func<IUnityObjectBase, bool> Filter { get; set; } = DefaultFilterMethod;
		#endregion

		#region Project Settings
		public UnityVersion Version { get; private set; }
		public BuildTarget Platform { get; private set; }
		public TransferInstructionFlags Flags { get; private set; }
		#endregion

		#region Default Filter
		/// <summary>
		/// The default filter that allows everything
		/// </summary>
		public static Func<IUnityObjectBase, bool> DefaultFilter { get; } = DefaultFilterMethod;
		private static bool DefaultFilterMethod(IUnityObjectBase asset) => true;
		#endregion

		public CoreConfiguration() => ResetToDefaultValues();

		public void SetProjectSettings(Layout.LayoutInfo info) => SetProjectSettings(info.Version, info.Platform, info.Flags);
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
			ExportRootPath = ExecutingDirectory.Combine("Ripped");
			ExportDependencies = false;
			BundledAssetsExportMode = BundledAssetsExportMode.GroupByBundleName;
			Filter = DefaultFilter;
		}

		public virtual void LogConfigurationValues()
		{
			Logger.Info(LogCategory.General, $"Configuration Settings:");
			Logger.Info(LogCategory.General, $"{nameof(ScriptContentLevel)}: {ScriptContentLevel}");
			Logger.Info(LogCategory.General, $"{nameof(StreamingAssetsMode)}: {StreamingAssetsMode}");
			Logger.Info(LogCategory.General, $"{nameof(ExportRootPath)}: {ExportRootPath}");
			Logger.Info(LogCategory.General, $"{nameof(ExportDependencies)}: {ExportDependencies}");
			Logger.Info(LogCategory.General, $"{nameof(BundledAssetsExportMode)}: {BundledAssetsExportMode}");
		}
	}
}
