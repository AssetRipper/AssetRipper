using System;

namespace UtinyRipper.Classes.Meshes
{
	/// <summary>
	/// Less than 5.0.0 version
	/// </summary>
	public enum ChannelType4
	{
		Vertex		= 0,
		Normal		= 1,
		Color		= 2,
		UV0			= 3,
		UV1			= 4,
		Tangent		= 5,
	}

	public static class ChannelType3Extensions
	{
		public static ChannelFormat GetFormat(this ChannelType4 _this)
		{
			switch (_this)
			{
				case ChannelType4.Vertex:
				case ChannelType4.Normal:
					return ChannelFormat.Float;

				case ChannelType4.Color:
					return ChannelFormat.Byte;

				case ChannelType4.UV0:
				case ChannelType4.UV1:
					return ChannelFormat.Float;

				case ChannelType4.Tangent:
					return ChannelFormat.Float;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ChannelType4 _this)
		{
			switch (_this)
			{
				case ChannelType4.Vertex:
				case ChannelType4.Normal:
					return 3;

				case ChannelType4.Color:
					return 4;

				case ChannelType4.UV0:
				case ChannelType4.UV1:
					return 2;

				case ChannelType4.Tangent:
					return 4;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static ChannelType ToChannelType(this ChannelType4 _this)
		{
			switch(_this)
			{
				case ChannelType4.Vertex:
					return ChannelType.Vertex;
				case ChannelType4.Normal:
					return ChannelType.Normal;
				case ChannelType4.Color:
					return ChannelType.Color;
				case ChannelType4.UV0:
					return ChannelType.UV0;
				case ChannelType4.UV1:
					return ChannelType.UV1;
				case ChannelType4.Tangent:
					return ChannelType.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetStride(this ChannelType4 _this)
		{
			ChannelFormat format = _this.GetFormat();
			int dimention = _this.GetDimention();
			return ChannelInfo.CalculateStride(format, dimention);
		}
	}
}
