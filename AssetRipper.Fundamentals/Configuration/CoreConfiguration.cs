using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Utils;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Configuration
{
	public class CoreConfiguration
	{
		public Dictionary<Type, object> settings = new Dictionary<Type, object>();

		public T? GetSetting<T>()
		{
			object? result = this.settings.GetValueOrDefault(typeof(T));

			if (result == null || result == default)
				return default(T);

			return (T?)result;
		}

		public void SetSetting(Type type, object value)
		{
			if (value == null)
				return;

			this.settings.Remove(type);
			this.settings.Add(type, value);
		}

		public void SetSetting<T>(object value)
		{
			SetSetting(typeof(T), value);
		}

		#region Import Settings
		/// <summary>
		/// Disabling scripts can allow some games to export when they previously did not.
		/// </summary>
		public bool DisableScriptImport => ScriptContentLevel == ScriptContentLevel.Level0;
		/// <summary>
		/// The level of scripts to export
		/// </summary>
		public ScriptContentLevel ScriptContentLevel
		{
			get { return this.GetSetting<ScriptContentLevel>(); }
			set { this.SetSetting<ScriptContentLevel>(value); }
		}
		/// <summary>
		/// Including the streaming assets directory can cause some games to fail while exporting.
		/// </summary>
		public bool IgnoreStreamingAssets
		{
			get => this.GetSetting<StreamingAssetsMode>() == StreamingAssetsMode.Ignore;
			set
			{
				this.SetSetting<StreamingAssetsMode>(value ? StreamingAssetsMode.Ignore : StreamingAssetsMode.Extract);
			}
		}
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
		public bool ExportDependencies
		{
			get { return this.GetSetting<ExportDependencies>() == Configuration.ExportDependencies.Export; }
			set { this.SetSetting<ExportDependencies>(value ? Configuration.ExportDependencies.Export : Configuration.ExportDependencies.Ignore); }
		}
		public BundledAssetsExportMode BundledAssetsExportMode
		{
			get { return this.GetSetting<BundledAssetsExportMode>(); }
			set { this.SetSetting<BundledAssetsExportMode>(value); }
		}
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
			IgnoreStreamingAssets = false;
			ExportRootPath = ExecutingDirectory.Combine("Ripped");
			ExportDependencies = false;
			BundledAssetsExportMode = BundledAssetsExportMode.GroupByBundleName;
			Filter = DefaultFilter;
		}

		public virtual void LogConfigurationValues()
		{
			Logger.Info(LogCategory.General, $"Configuration Settings:");
			Logger.Info(LogCategory.General, $"{nameof(ScriptContentLevel)}: {ScriptContentLevel}");
			Logger.Info(LogCategory.General, $"{nameof(StreamingAssetsMode)}: {this.GetSetting<StreamingAssetsMode>()}");
			Logger.Info(LogCategory.General, $"{nameof(ExportRootPath)}: {ExportRootPath}");
			Logger.Info(LogCategory.General, $"{nameof(ExportDependencies)}: {ExportDependencies}");
			Logger.Info(LogCategory.General, $"{nameof(BundledAssetsExportMode)}: {BundledAssetsExportMode}");
		}
	}
}
