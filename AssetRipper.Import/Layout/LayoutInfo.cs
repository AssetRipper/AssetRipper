using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Import.Layout
{
	/// <summary>
	/// A class for holding the Version, Platform, and Transfer Instruction Flags
	/// </summary>
	public sealed record class LayoutInfo
	{
		/// <summary>
		/// 2.1.0 and greater
		/// The alignment concept was first introduced only in v2.1.0
		/// </summary>
		public bool IsAlign { get; }
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public bool IsAlignArrays { get; }
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public bool IsStructSerializable { get; }

		public UnityVersion Version { get; }
		public BuildTarget Platform { get; }
		public TransferInstructionFlags Flags { get; }

		public LayoutInfo(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
			IsAlign = Version.IsGreaterEqual(2, 1);
			IsAlignArrays = Version.IsGreaterEqual(2017);
			IsStructSerializable = Version.IsGreaterEqual(4, 5);
		}

		public override string ToString()
		{
			return $"v{Version} {Platform} [{Flags}]";
		}
	}
}
