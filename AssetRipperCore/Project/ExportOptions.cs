using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.IO.Asset;
using System;
using Object = AssetRipper.Core.Classes.Object.Object;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Project
{
	public sealed class ExportOptions
	{
		public ExportOptions(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}

		private static bool DefaultFilter(Object asset)
		{
			return true;
		}

		public UnityVersion Version { get; }
		public Platform Platform { get; }
		public TransferInstructionFlags Flags { get; }

		/// <summary>
		/// Should objects get exported with dependencies or without
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
		public Func<Object, bool> Filter { get; set; } = DefaultFilter;
	}
}
