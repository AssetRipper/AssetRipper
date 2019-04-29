using System;

namespace uTinyRipper.Classes.Meshes
{
	public enum ChannelFormatV4 : byte
	{
		Float		= 0,
		Float16		= 1,
		Color		= 2,
		Byte		= 3,
	}

	public static class ChannelFormatV4Extension
	{
		public static ChannelFormat ToChannelFormat(this ChannelFormatV4 _this)
		{
			switch (_this)
			{
				case ChannelFormatV4.Float:
					return ChannelFormat.Float;
				case ChannelFormatV4.Float16:
					return ChannelFormat.Float16;
				case ChannelFormatV4.Color:
					return ChannelFormat.Color;
				case ChannelFormatV4.Byte:
					return ChannelFormat.Byte;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
