using System;

namespace uTinyRipper.Classes.Meshes
{
	public enum ChannelType
	{
		Vertex,
		Normal,
		Tangent,
		/// <summary>
		/// 4.x.x Color. size 4bytes, dimention 1
		/// </summary>
		Color4,
		/// <summary>
		/// >= 5.x.x Color. size 1byte, dimention 4
		/// </summary>
		Color,
		UV0,
		UV1,
		UV2,
		UV3,
		UV4,
		UV5,
		UV6,
		UV7,
		SkinWeight,
		SkinBoneIndex,
	}

	public static class ChannelTypeExtensions
	{
		public static ChannelFormat GetFormat(this ChannelType _this)
		{
			switch (_this)
			{
				case ChannelType.Vertex:
					return ChannelFormat.Float;
				case ChannelType.Normal:
					return ChannelFormat.Float;
				case ChannelType.Tangent:
					return ChannelFormat.Float;
				case ChannelType.Color4:
					return ChannelFormat.Color;
				case ChannelType.Color:
					return ChannelFormat.Byte;

				case ChannelType.UV0:
				case ChannelType.UV1:
				case ChannelType.UV2:
				case ChannelType.UV3:
				case ChannelType.UV4:
				case ChannelType.UV5:
				case ChannelType.UV6:
				case ChannelType.UV7:
					return ChannelFormat.Float;

				case ChannelType.SkinWeight:
					return ChannelFormat.Float;
				case ChannelType.SkinBoneIndex:
					return ChannelFormat.Int;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ChannelType _this)
		{
			switch (_this)
			{
				case ChannelType.Vertex:
					return 3;
				case ChannelType.Normal:
					return 3;
				case ChannelType.Tangent:
					return 4;
				case ChannelType.Color4:
					return 1;
				case ChannelType.Color:
					return 4;

				case ChannelType.UV0:
				case ChannelType.UV1:
				case ChannelType.UV2:
				case ChannelType.UV3:
				case ChannelType.UV4:
				case ChannelType.UV5:
				case ChannelType.UV6:
				case ChannelType.UV7:
					return 2;

				case ChannelType.SkinWeight:
					return 4;
				case ChannelType.SkinBoneIndex:
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

		public static ChannelTypeV5 ToChannelTypeV5(this ChannelType _this)
		{
			switch (_this)
			{
				case ChannelType.Vertex:
					return ChannelTypeV5.Vertex;
				case ChannelType.Normal:
					return ChannelTypeV5.Normal;
				case ChannelType.Color:
					return ChannelTypeV5.Color;
				case ChannelType.UV0:
					return ChannelTypeV5.UV0;
				case ChannelType.UV1:
					return ChannelTypeV5.UV1;
				case ChannelType.UV2:
					return ChannelTypeV5.UV2;
				case ChannelType.UV3:
					return ChannelTypeV5.UV3;
				case ChannelType.Tangent:
					return ChannelTypeV5.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
