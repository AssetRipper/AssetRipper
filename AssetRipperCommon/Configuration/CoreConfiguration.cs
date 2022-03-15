using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Utils;
using System;

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
		/// The path to create a new unity project in
		/// </summary>
		public string ExportPath { get; set; }
		/// <summary>
		/// Should objects get exported with dependencies or without?
		/// </summary>
		public bool ExportDependencies { get; set; }
		/// <summary>
		/// Export asset bundle content to its original path instead of the Asset_Bundle directory
		/// </summary>
		public bool IgnoreAssetBundleContentPaths { get; set; }
		/// <summary>
		/// A function to determine if an object is allowed to be exported.<br/>
		/// Set by default to allow everything.
		/// </summary>
		public Func<IUnityObjectBase, bool> Filter { get; set; }
		#endregion

		#region Project Settings
		public UnityVersion Version { get; private set; }
		public Platform Platform { get; private set; }
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

		internal void SetProjectSettings(Layout.LayoutInfo info) => SetProjectSettings(info.Version, info.Platform, info.Flags);
		internal void SetProjectSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}

		public virtual void ResetToDefaultValues()
		{
			ScriptContentLevel = ScriptContentLevel.Level2;
			StreamingAssetsMode = StreamingAssetsMode.Extract;
			ExportPath = ExecutingDirectory.Combine("Ripped");
			ExportDependencies = false;
			IgnoreAssetBundleContentPaths = false;
			Filter = DefaultFilter;
		}

		public virtual void LogConfigurationValues()
		{
			Logger.Info(LogCategory.General, $"Configuration Settings:");
			Logger.Info(LogCategory.General, $"{nameof(ScriptContentLevel)}: {ScriptContentLevel}");
			Logger.Info(LogCategory.General, $"{nameof(StreamingAssetsMode)}: {StreamingAssetsMode}");
			Logger.Info(LogCategory.General, $"{nameof(ExportPath)}: {ExportPath}");
			Logger.Info(LogCategory.General, $"{nameof(ExportDependencies)}: {ExportDependencies}");
			Logger.Info(LogCategory.General, $"{nameof(IgnoreAssetBundleContentPaths)}: {IgnoreAssetBundleContentPaths}");
		}
	}
}
