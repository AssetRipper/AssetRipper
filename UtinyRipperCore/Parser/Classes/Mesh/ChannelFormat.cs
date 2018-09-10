using System;

namespace UtinyRipper.Classes.Meshes
{
	public enum ChannelFormat : byte
	{
		Float		= 0,
		Float16		= 1,
		/// <summary>
		/// 4.x.x.
		/// </summary>
		Color		= 2,
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		Byte		= 2,
		/// <summary>
		/// 4.x.x
		/// </summary>
		ByteV4		= 3,
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
				case ChannelFormat.Byte:
				case ChannelFormat.ByteV4:
					return 1;
				case ChannelFormat.Int:
					return 4;

				default:
					throw new Exception(_this.ToString());
			}
		}
	}
}
