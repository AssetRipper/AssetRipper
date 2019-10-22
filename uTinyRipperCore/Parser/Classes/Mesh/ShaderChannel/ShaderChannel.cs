using System;

namespace uTinyRipper.Classes.Meshes
{
	public enum ShaderChannel
	{
		Vertex,
		Normal,
		Tangent,
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

	public static class ShaderChannelExtensions
	{
		public static int GetChannelCount(Version version)
		{
			if (version.IsLess(5))
			{
				return 6;
			}
			else if (version.IsLess(2018))
			{
				return 8;
			}
			else
			{
				return 14;
			}
		}

		public static VertexFormat GetVertexFormat(this ShaderChannel _this, Version version)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return VertexFormat.Float;
				case ShaderChannel.Normal:
					return VertexFormat.Float;
				case ShaderChannel.Tangent:
					return VertexFormat.Float;
				case ShaderChannel.Color:
					return version.IsLess(5) ? VertexFormat.Color : VertexFormat.Byte;

				case ShaderChannel.UV0:
				case ShaderChannel.UV1:
				case ShaderChannel.UV2:
				case ShaderChannel.UV3:
				case ShaderChannel.UV4:
				case ShaderChannel.UV5:
				case ShaderChannel.UV6:
				case ShaderChannel.UV7:
					return VertexFormat.Float;

				case ShaderChannel.SkinWeight:
					return VertexFormat.Float;
				case ShaderChannel.SkinBoneIndex:
					return VertexFormat.Int;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetDimention(this ShaderChannel _this, Version version)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return 3;
				case ShaderChannel.Normal:
					return 3;
				case ShaderChannel.Tangent:
					return 4;
				case ShaderChannel.Color:
					return version.IsLess(5) ? (byte)1 : (byte)4;

				case ShaderChannel.UV0:
				case ShaderChannel.UV1:
				case ShaderChannel.UV2:
				case ShaderChannel.UV3:
				case ShaderChannel.UV4:
				case ShaderChannel.UV5:
				case ShaderChannel.UV6:
				case ShaderChannel.UV7:
					return 2;

				case ShaderChannel.SkinWeight:
				case ShaderChannel.SkinBoneIndex:
					throw new Exception($"Skin's dimention is varying");

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static byte GetStride(this ShaderChannel _this)
		{
			// since sizeof(Color) * 1 == sizeof(Byte) * 4, we can omit version
			VertexFormat format = _this.GetVertexFormat(Version.MinVersion);
			int dimention = _this.GetDimention(Version.MinVersion);
			return format.CalculateStride(dimention);
		}

		public static bool HasChannel(this ShaderChannel _this, Version version)
		{
			if (version.IsLess(5))
			{
				return _this <= ShaderChannel.UV1;
			}
			else if (version.IsLess(2018))
			{
				return _this <= ShaderChannel.UV4;
			}
			else
			{
				return true;
			}
		}

		public static int ToChannel(this ShaderChannel _this, Version version)
		{
			if (version.IsLess(5))
			{
				return (int)ToShaderChannelV4(_this);
			}
			else if (version.IsLess(2018))
			{
				return (int)ToShaderChannelV5(_this);
			}
			else
			{
				return (int)ToShaderChannelV2018(_this);
			}
		}

		public static ShaderChannelV4 ToShaderChannelV4(this ShaderChannel _this)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return ShaderChannelV4.Vertex;
				case ShaderChannel.Normal:
					return ShaderChannelV4.Normal;
				case ShaderChannel.Color:
					return ShaderChannelV4.Color;
				case ShaderChannel.UV0:
					return ShaderChannelV4.UV0;
				case ShaderChannel.UV1:
					return ShaderChannelV4.UV1;
				case ShaderChannel.Tangent:
					return ShaderChannelV4.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static ShaderChannelV5 ToShaderChannelV5(this ShaderChannel _this)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return ShaderChannelV5.Vertex;
				case ShaderChannel.Normal:
					return ShaderChannelV5.Normal;
				case ShaderChannel.Color:
					return ShaderChannelV5.Color;
				case ShaderChannel.UV0:
					return ShaderChannelV5.UV0;
				case ShaderChannel.UV1:
					return ShaderChannelV5.UV1;
				case ShaderChannel.UV2:
					return ShaderChannelV5.UV2;
				case ShaderChannel.UV3:
					return ShaderChannelV5.UV3;
				case ShaderChannel.Tangent:
					return ShaderChannelV5.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}

		public static ShaderChannelV2018 ToShaderChannelV2018(this ShaderChannel _this)
		{
			switch (_this)
			{
				case ShaderChannel.Vertex:
					return ShaderChannelV2018.Vertex;
				case ShaderChannel.Normal:
					return ShaderChannelV2018.Normal;
				case ShaderChannel.Tangent:
					return ShaderChannelV2018.Tangent;
				case ShaderChannel.Color:
					return ShaderChannelV2018.Color;
				case ShaderChannel.UV0:
					return ShaderChannelV2018.UV0;
				case ShaderChannel.UV1:
					return ShaderChannelV2018.UV1;
				case ShaderChannel.UV2:
					return ShaderChannelV2018.UV2;
				case ShaderChannel.UV3:
					return ShaderChannelV2018.UV3;
				case ShaderChannel.UV4:
					return ShaderChannelV2018.UV4;
				case ShaderChannel.UV5:
					return ShaderChannelV2018.UV5;
				case ShaderChannel.UV6:
					return ShaderChannelV2018.UV6;
				case ShaderChannel.UV7:
					return ShaderChannelV2018.UV7;
				case ShaderChannel.SkinWeight:
					return ShaderChannelV2018.SkinWeight;
				case ShaderChannel.SkinBoneIndex:
					return ShaderChannelV2018.SkinBoneIndex;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
