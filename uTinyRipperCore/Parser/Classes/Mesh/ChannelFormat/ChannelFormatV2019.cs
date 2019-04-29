using System;

namespace uTinyRipper.Classes.Meshes
{
	public enum ChannelFormatV2019 : byte
	{
		Float		= 0,
		Float16		= 1,
		Byte		= 2,
		Int			= 10,
	}

	public static class ChannelFormat2019Extension
	{
		public static ChannelFormat ToChannelFormat(this ChannelFormatV2019 _this)
		{
			switch (_this)
			{
				case ChannelFormatV2019.Float:
					return ChannelFormat.Float;
				case ChannelFormatV2019.Float16:
					return ChannelFormat.Float16;
				case ChannelFormatV2019.Byte:
					return ChannelFormat.Byte;
				case ChannelFormatV2019.Int:
					return ChannelFormat.Int;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
