using System;

namespace UtinyRipper.Classes.Meshes
{
	/// <summary>
	/// Less than 4.0.0 version
	/// </summary>
	public enum ChannelType3
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
		public static ChannelFormat GetFormat(this ChannelType3 _this)
		{
			switch (_this)
			{
				case ChannelType3.Vertex:
				case ChannelType3.Normal:
					return ChannelFormat.Float;

				case ChannelType3.Color:
					return ChannelFormat.Byte;

				case ChannelType3.UV0:
				case ChannelType3.UV1:
					return ChannelFormat.Float;

				case ChannelType3.Tangent:
					return ChannelFormat.Float;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ChannelType3 _this)
		{
			switch (_this)
			{
				case ChannelType3.Vertex:
				case ChannelType3.Normal:
					return 3;

				case ChannelType3.Color:
					return 4;

				case ChannelType3.UV0:
				case ChannelType3.UV1:
					return 2;

				case ChannelType3.Tangent:
					return 4;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static ChannelType ToChannelType(this ChannelType3 _this)
		{
			switch(_this)
			{
				case ChannelType3.Vertex:
					return ChannelType.Vertex;
				case ChannelType3.Normal:
					return ChannelType.Normal;
				case ChannelType3.Color:
					return ChannelType.Color;
				case ChannelType3.UV0:
					return ChannelType.UV0;
				case ChannelType3.UV1:
					return ChannelType.UV1;
				case ChannelType3.Tangent:
					return ChannelType.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetStride(this ChannelType3 _this)
		{
			ChannelFormat format = _this.GetFormat();
			int dimention = _this.GetDimention();
			return ChannelInfo.CalculateStride(format, dimention);
		}
	}
}
