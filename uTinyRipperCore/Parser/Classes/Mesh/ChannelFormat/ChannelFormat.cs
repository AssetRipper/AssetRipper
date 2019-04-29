using System;

namespace uTinyRipper.Classes.Meshes
{
	public enum ChannelFormat
	{
		Float,
		Float16,
		Color,
		Byte,
		Int,
	}

	public static class ChannelFormatExtension
	{
		public static int GetSize(this ChannelFormat _this)
		{
			switch (_this)
			{
				case ChannelFormat.Float:
					return 4;
				case ChannelFormat.Float16:
					return 2;
				case ChannelFormat.Color:
					return 4;
				case ChannelFormat.Byte:
					return 1;
				case ChannelFormat.Int:
					return 4;

				default:
					throw new Exception(_this.ToString());
			}
		}

		public static ChannelFormatV5 ToChannelFormatV5(this ChannelFormat _this)
		{
			switch (_this)
			{
				case ChannelFormat.Float:
					return ChannelFormatV5.Float;
				case ChannelFormat.Float16:
					return ChannelFormatV5.Float16;
				case ChannelFormat.Byte:
					return ChannelFormatV5.Byte;
				case ChannelFormat.Int:
					return ChannelFormatV5.Int;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
