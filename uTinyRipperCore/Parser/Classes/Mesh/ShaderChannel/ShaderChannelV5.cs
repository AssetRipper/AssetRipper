using System;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// 5.0.0 to 2017.x versions
	/// </summary>
	public enum ShaderChannelV5
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
		public static ShaderChannel ToShaderChannel(this ShaderChannelV5 _this)
		{
			switch (_this)
			{
				case ShaderChannelV5.Vertex:
					return ShaderChannel.Vertex;
				case ShaderChannelV5.Normal:
					return ShaderChannel.Normal;
				case ShaderChannelV5.Color:
					return ShaderChannel.Color;
				case ShaderChannelV5.UV0:
					return ShaderChannel.UV0;
				case ShaderChannelV5.UV1:
					return ShaderChannel.UV1;
				case ShaderChannelV5.UV2:
					return ShaderChannel.UV2;
				case ShaderChannelV5.UV3:
					return ShaderChannel.UV3;
				case ShaderChannelV5.Tangent:
					return ShaderChannel.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
