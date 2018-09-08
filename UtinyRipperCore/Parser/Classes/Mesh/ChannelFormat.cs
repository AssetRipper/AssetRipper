using System;

namespace UtinyRipper.Classes.Meshes
{
	public enum ChannelFormat : byte
	{
		Float		= 0,
		Float16		= 1,
		Color		= 2,
		Byte		= 3,
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
				case ChannelFormat.Float16:
					return 2;
				case ChannelFormat.Color:
					return 1;
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
