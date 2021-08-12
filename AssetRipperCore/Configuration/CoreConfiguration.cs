using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Utils;
using System;

namespace AssetRipper.Core.Configuration
{
	public class CoreConfiguration
	{
		#region Import Settings
		/// <summary>
		/// How do scripts get imported?
		/// </summary>
		public ScriptImportMode ScriptImportMode { get; set; } = ScriptImportMode.Default;
		#endregion

		#region Export Settings
		/// <summary>
		/// The path to create a new unity project in
		/// </summary>
		public string ExportPath { get; set; } = DirectoryUtils.CombineWithExecutingDirectory("Ripped");
		/// <summary>
		/// Should objects get exported with dependencies or without?
		/// </summary>
		public bool ExportDependencies { get; set; }
		/// <summary>
		/// Export asset bundle content to its original path instead of AssetBundle directory
		/// </summary>
		public bool KeepAssetBundleContentPath { get; set; }
		/// <summary>
		/// A function to determine if an object is allowed to be exported.<br/>
		/// Set by default to allow everything.
		/// </summary>
		public Func<Classes.Object.Object, bool> Filter { get; set; } = DefaultFilter;
		/// <summary>
		/// How are text assets exported?
		/// </summary>
		public TextExportMode TextExportMode { get; set; } = TextExportMode.Bytes;
		#endregion

		#region Project Settings
		public UnityVersion Version { get; private set; }
		public Platform Platform { get; private set; }
		public TransferInstructionFlags Flags { get; private set; }
		#endregion

		private static bool DefaultFilter(Classes.Object.Object asset) => true;

		internal void SetProjectSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}
	}
}
