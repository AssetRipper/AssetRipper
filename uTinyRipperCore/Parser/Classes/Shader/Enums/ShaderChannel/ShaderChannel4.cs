using System;

namespace uTinyRipper.Classes.Shaders
{
	/// <summary>
	/// Less than 5.0.0 version
	/// </summary>
	public enum ShaderChannel4
	{
		Vertex	= 0,
		Normal	= 1,
		Color	= 2,
		UV0		= 3,
		UV1		= 4,
		Tangent	= 5,
	}

	public static class ShaderChannelV4Extensions
	{
		public static ShaderChannel ToShaderChannel(this ShaderChannel4 _this)
		{
			switch(_this)
			{
				case ShaderChannel4.Vertex:
					return ShaderChannel.Vertex;
				case ShaderChannel4.Normal:
					return ShaderChannel.Normal;
				case ShaderChannel4.Color:
					return ShaderChannel.Color;
				case ShaderChannel4.UV0:
					return ShaderChannel.UV0;
				case ShaderChannel4.UV1:
					return ShaderChannel.UV1;
				case ShaderChannel4.Tangent:
					return ShaderChannel.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
