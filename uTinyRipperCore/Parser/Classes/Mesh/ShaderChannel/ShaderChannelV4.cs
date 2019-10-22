using System;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// Less than 5.0.0 version
	/// </summary>
	public enum ShaderChannelV4
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
		public static ShaderChannel ToShaderChannel(this ShaderChannelV4 _this)
		{
			switch(_this)
			{
				case ShaderChannelV4.Vertex:
					return ShaderChannel.Vertex;
				case ShaderChannelV4.Normal:
					return ShaderChannel.Normal;
				case ShaderChannelV4.Color:
					return ShaderChannel.Color;
				case ShaderChannelV4.UV0:
					return ShaderChannel.UV0;
				case ShaderChannelV4.UV1:
					return ShaderChannel.UV1;
				case ShaderChannelV4.Tangent:
					return ShaderChannel.Tangent;

				default:
					throw new Exception($"Unsupported channel type {_this}");
			}
		}
	}
}
