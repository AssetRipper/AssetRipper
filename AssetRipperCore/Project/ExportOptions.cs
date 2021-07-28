using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using System;
using UnityObject = AssetRipper.Classes.Object.UnityObject;
using Version = AssetRipper.Parser.Files.Version;

namespace AssetRipper.Project
{
	public sealed class ExportOptions
	{
		public ExportOptions(Version version, Platform platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}

		private static bool DefaultFilter(UnityObject asset)
		{
			return true;
		}

		public Version Version { get; }
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
		public Func<UnityObject, bool> Filter { get; set; } = DefaultFilter;
	}
}
