namespace uTinyRipper.Layout
{
	public sealed class LayoutInfo
	{
		public LayoutInfo(Version version, Platform platform, TransferInstructionFlags flags)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}

		public static bool operator == (LayoutInfo lhs, LayoutInfo rhs)
		{
			if (lhs.Version != rhs.Version)
			{
				return false;
			}
			if (lhs.Platform != rhs.Platform)
			{
				return false;
			}
			if (lhs.Flags != rhs.Flags)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(LayoutInfo lhs, LayoutInfo rhs)
		{
			if (lhs.Version != rhs.Version)
			{
				return true;
			}
			if (lhs.Platform != rhs.Platform)
			{
				return true;
			}
			if (lhs.Flags != rhs.Flags)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 647;
			unchecked
			{
				hash = hash + 1063 * Version.GetHashCode();
				hash = hash * 347 + Platform.GetHashCode();
				hash = hash * 557 + Flags.GetHashCode();
			}
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj is LayoutInfo info)
			{
				return info == this;
			}
			return false;
		}

		public override string ToString()
		{
			return $"v{Version} {Platform} [{Flags}]";
		}

		public Version Version { get; }
		public Platform Platform { get; }
		public TransferInstructionFlags Flags { get; }
	}
}
