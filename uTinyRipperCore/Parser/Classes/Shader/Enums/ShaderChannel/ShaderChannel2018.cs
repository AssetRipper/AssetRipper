using System;

namespace uTinyRipper.Classes.Shaders
{
	/// <summary>
	/// 2018.1 and greater version
	/// </summary>
	public enum ShaderChannel2018
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

	public static class ShaderChannelV2018Extensions
	{
		public static ShaderChannel ToShaderChannel(this ShaderChannel2018 _this)
		{
			switch (_this)
			{
				case ShaderChannel2018.Vertex:
					return ShaderChannel.Vertex;
				case ShaderChannel2018.Normal:
					return ShaderChannel.Normal;
				case ShaderChannel2018.Tangent:
					return ShaderChannel.Tangent;
				case ShaderChannel2018.Color:
					return ShaderChannel.Color;
				case ShaderChannel2018.UV0:
					return ShaderChannel.UV0;
				case ShaderChannel2018.UV1:
					return ShaderChannel.UV1;
				case ShaderChannel2018.UV2:
					return ShaderChannel.UV2;
				case ShaderChannel2018.UV3:
					return ShaderChannel.UV3;
				case ShaderChannel2018.UV4:
					return ShaderChannel.UV4;
				case ShaderChannel2018.UV5:
					return ShaderChannel.UV5;
				case ShaderChannel2018.UV6:
					return ShaderChannel.UV6;
				case ShaderChannel2018.UV7:
					return ShaderChannel.UV7;
				case ShaderChannel2018.SkinWeight:
					return ShaderChannel.SkinWeight;
				case ShaderChannel2018.SkinBoneIndex:
					return ShaderChannel.SkinBoneIndex;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
