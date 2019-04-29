using System;

namespace uTinyRipper.Classes.Meshes
{
	public enum ChannelFormatV5 : byte
	{
		Float		= 0,
		Float16		= 1,
		Byte		= 2,
		Int			= 11,
	}

	public static class ChannelFormatV5Extension
	{
		public static ChannelFormat ToChannelFormat(this ChannelFormatV5 _this)
		{
			switch (_this)
			{
				case ChannelFormatV5.Float:
					return ChannelFormat.Float;
				case ChannelFormatV5.Float16:
					return ChannelFormat.Float16;
				case ChannelFormatV5.Byte:
					return ChannelFormat.Byte;
				case ChannelFormatV5.Int:
					return ChannelFormat.Int;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
