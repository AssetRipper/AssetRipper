using System;

namespace UtinyRipper.Classes.Meshes
{
	public enum ChannelType
	{
		Vertex		= 0,
		Normal		= 1,
		Color		= 2,
		UV0			= 3,
		UV1			= 4,
		UV2			= 5,
		UV3			= 6,
		Tangent		= 7,
	}

	public static class ChannelTypeExtensions
	{
		public static ChannelFormat GetFormat(this ChannelType _this)
		{
			switch(_this)
			{
				case ChannelType.Vertex:
				case ChannelType.Normal:
					return ChannelFormat.Float;

				case ChannelType.Color:
					return ChannelFormat.Color;

				case ChannelType.UV0:
				case ChannelType.UV1:
				case ChannelType.UV2:
				case ChannelType.UV3:
					return ChannelFormat.Float;

				case ChannelType.Tangent:
					return ChannelFormat.Float;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ChannelType _this)
		{
			switch (_this)
			{
				case ChannelType.Vertex:
				case ChannelType.Normal:
					return 3;

				case ChannelType.Color:
					return 4;

				case ChannelType.UV0:
				case ChannelType.UV1:
				case ChannelType.UV2:
				case ChannelType.UV3:
					return 2;

				case ChannelType.Tangent:
					return 4;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetStride(this ChannelType _this)
		{
			ChannelFormat format = _this.GetFormat();
			int dimention = _this.GetDimention();
			return ChannelInfo.CalculateStride(format, dimention);
		}
	}
}
