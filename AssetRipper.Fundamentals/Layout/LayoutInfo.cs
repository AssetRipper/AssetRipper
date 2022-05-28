using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Layout
{
	/// <summary>
	/// A class for holding the Version, Platform, and Transfer Instruction Flags
	/// </summary>
	public sealed class LayoutInfo : IEquatable<LayoutInfo>
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

		public override bool Equals(object? obj)
		{
			return Equals(obj as LayoutInfo);
		}

		public bool Equals(LayoutInfo? other)
		{
			return other is not null &&
				   Version.Equals(other.Version) &&
				   Platform == other.Platform &&
				   Flags == other.Flags;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Version, Platform, Flags);
		}

		public static bool operator ==(LayoutInfo left, LayoutInfo right)
		{
			return EqualityComparer<LayoutInfo>.Default.Equals(left, right);
		}

		public static bool operator !=(LayoutInfo left, LayoutInfo right)
		{
			return !(left == right);
		}
	}
}
