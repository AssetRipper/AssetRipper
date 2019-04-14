using System;

namespace uTinyRipper.Classes.Shaders
{
	public enum ShaderType
	{
		None		= 0,
		Vertex		= 1,
		Fragment	= 2,
		Geometry	= 3,
		Hull		= 4,
		Domain		= 5,
		TypeCount	= 6,
	}

	public static class ShaderTypeExtensions
	{
		public static string ToProgramTypeString(this ShaderType _this)
		{
			switch (_this)
			{
				case ShaderType.Vertex:
					return "vp";
				case ShaderType.Fragment:
					return "fp";
				case ShaderType.Geometry:
					return "gp";
				case ShaderType.Hull:
					return "hp";
				case ShaderType.Domain:
					return "dp";

				default:
					throw new NotSupportedException($"ShaderType {_this} isn't supported");
			}
		}
	}
}
