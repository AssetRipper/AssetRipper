using System;

namespace UtinyRipper.Classes.Meshes
{
	public enum ChannelFormat : byte
	{
		Float		= 0,
		HalfFloat	= 1,
		Byte		= 2,
		Int			= 11,
	}

	public static class ChannelFormatExtension
	{
		public static int GetSize(this ChannelFormat _this)
		{
			switch(_this)
			{
				case ChannelFormat.Float:
					return 4;
				case ChannelFormat.HalfFloat:
					return 2;
				case ChannelFormat.Byte:
					return 1;
				case ChannelFormat.Int:
					return 4;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
