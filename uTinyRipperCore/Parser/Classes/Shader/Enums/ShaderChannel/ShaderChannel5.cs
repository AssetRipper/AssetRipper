using System;

namespace uTinyRipper.Classes.Shaders
{
	/// <summary>
	/// 5.0.0 to 2017.x versions
	/// </summary>
	public enum ShaderChannel5
	{
		Vertex	= 0,
		Normal	= 1,
		Color	= 2,
		UV0		= 3,
		UV1		= 4,
		UV2		= 5,
		UV3		= 6,
		Tangent	= 7,
	}

	public static class ShaderChannelV5Extensions
	{
		public static ShaderChannel ToShaderChannel(this ShaderChannel5 _this)
		{
			switch (_this)
			{
				case ShaderChannel5.Vertex:
					return ShaderChannel.Vertex;
				case ShaderChannel5.Normal:
					return ShaderChannel.Normal;
				case ShaderChannel5.Color:
					return ShaderChannel.Color;
				case ShaderChannel5.UV0:
					return ShaderChannel.UV0;
				case ShaderChannel5.UV1:
					return ShaderChannel.UV1;
				case ShaderChannel5.UV2:
					return ShaderChannel.UV2;
				case ShaderChannel5.UV3:
					return ShaderChannel.UV3;
				case ShaderChannel5.Tangent:
					return ShaderChannel.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
