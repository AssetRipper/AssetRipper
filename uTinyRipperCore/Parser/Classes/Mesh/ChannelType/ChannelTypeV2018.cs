using System;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// 2018.1 and greater version
	/// </summary>
	public enum ChannelTypeV2018
	{
		Vertex			= 0,
		Normal			= 1,
		Tangent			= 2,
		Color			= 3,
		UV0				= 4,
		UV1				= 5,
		UV2				= 6,
		UV3				= 7,
		UV4				= 8,
		UV5				= 9,
		UV6				= 10,
		UV7				= 11,
		SkinWeight		= 12,
		SkinBoneIndex	= 13,
	}

	public static class ChannelTypeV2018Extensions
	{
		public static ChannelFormat GetFormat(this ChannelTypeV2018 _this)
		{
			switch (_this)
			{
				case ChannelTypeV2018.Vertex:
				case ChannelTypeV2018.Normal:
					return ChannelFormat.Float;

				case ChannelTypeV2018.Tangent:
					return ChannelFormat.Float;

				case ChannelTypeV2018.Color:
					return ChannelFormat.Color;

				case ChannelTypeV2018.UV0:
				case ChannelTypeV2018.UV1:
				case ChannelTypeV2018.UV2:
				case ChannelTypeV2018.UV3:
				case ChannelTypeV2018.UV4:
				case ChannelTypeV2018.UV5:
				case ChannelTypeV2018.UV6:
				case ChannelTypeV2018.UV7:
					return ChannelFormat.Float;

				case ChannelTypeV2018.SkinWeight:
					return ChannelFormat.Float;
				case ChannelTypeV2018.SkinBoneIndex:
					return ChannelFormat.Int;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ChannelTypeV2018 _this)
		{
			switch (_this)
			{
				case ChannelTypeV2018.Vertex:
				case ChannelTypeV2018.Normal:
					return 3;

				case ChannelTypeV2018.Tangent:
					return 4;

				case ChannelTypeV2018.Color:
					return 4;

				case ChannelTypeV2018.UV0:
				case ChannelTypeV2018.UV1:
				case ChannelTypeV2018.UV2:
				case ChannelTypeV2018.UV3:
				case ChannelTypeV2018.UV4:
				case ChannelTypeV2018.UV5:
				case ChannelTypeV2018.UV6:
				case ChannelTypeV2018.UV7:
					return 2;

				case ChannelTypeV2018.SkinWeight:
					return 4;

				case ChannelTypeV2018.SkinBoneIndex:
					return 4;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static ChannelType ToChannelType(this ChannelTypeV2018 _this)
		{
			switch (_this)
			{
				case ChannelTypeV2018.Vertex:
					return ChannelType.Vertex;
				case ChannelTypeV2018.Normal:
					return ChannelType.Normal;
				case ChannelTypeV2018.Tangent:
					return ChannelType.Tangent;
				case ChannelTypeV2018.Color:
					return ChannelType.Color;
				case ChannelTypeV2018.UV0:
					return ChannelType.UV0;
				case ChannelTypeV2018.UV1:
					return ChannelType.UV1;
				case ChannelTypeV2018.UV2:
					return ChannelType.UV2;
				case ChannelTypeV2018.UV3:
					return ChannelType.UV3;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetStride(this ChannelTypeV2018 _this)
		{
			ChannelFormat format = _this.GetFormat();
			int dimention = _this.GetDimention();
			return ChannelInfo.CalculateStride(format, dimention);
		}
	}
}
