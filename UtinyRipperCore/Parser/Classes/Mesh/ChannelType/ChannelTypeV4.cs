using System;

namespace UtinyRipper.Classes.Meshes
{
	/// <summary>
	/// Less than 5.0.0 version
	/// </summary>
	public enum ChannelTypeV4
	{
		Vertex		= 0,
		Normal		= 1,
		Color		= 2,
		UV0			= 3,
		UV1			= 4,
		Tangent		= 5,
	}

	public static class ChannelTypeV3Extensions
	{
		public static ChannelFormat GetFormat(this ChannelTypeV4 _this)
		{
			switch (_this)
			{
				case ChannelTypeV4.Vertex:
				case ChannelTypeV4.Normal:
					return ChannelFormat.Float;

				case ChannelTypeV4.Color:
					return ChannelFormat.Color;

				case ChannelTypeV4.UV0:
				case ChannelTypeV4.UV1:
					return ChannelFormat.Float;

				case ChannelTypeV4.Tangent:
					return ChannelFormat.Float;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ChannelTypeV4 _this)
		{
			switch (_this)
			{
				case ChannelTypeV4.Vertex:
				case ChannelTypeV4.Normal:
					return 3;

				case ChannelTypeV4.Color:
					return 4;

				case ChannelTypeV4.UV0:
				case ChannelTypeV4.UV1:
					return 2;

				case ChannelTypeV4.Tangent:
					return 4;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static ChannelType ToChannelType(this ChannelTypeV4 _this)
		{
			switch(_this)
			{
				case ChannelTypeV4.Vertex:
					return ChannelType.Vertex;
				case ChannelTypeV4.Normal:
					return ChannelType.Normal;
				case ChannelTypeV4.Color:
					return ChannelType.Color;
				case ChannelTypeV4.UV0:
					return ChannelType.UV0;
				case ChannelTypeV4.UV1:
					return ChannelType.UV1;
				case ChannelTypeV4.Tangent:
					return ChannelType.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetStride(this ChannelTypeV4 _this)
		{
			ChannelFormat format = _this.GetFormat();
			int dimention = _this.GetDimention();
			return ChannelInfo.CalculateStride(format, dimention);
		}
	}
}
