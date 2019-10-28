using System;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.Converters
{
	public sealed class ExportOptions
	{
		public ExportOptions(Version version, Platform platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}

		private static bool DefaultFilter(Object asset)
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
		public Func<Object, bool> Filter { get; set; } = DefaultFilter;
	}
}
