using System;

namespace UtinyRipper.Classes.Shaders
{
	public enum ShaderType
	{
		None = 0,
		Vertex,
		Fragment,
		Geometry,
		Hull,
		Domain,
		TypeCount,
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
#warning untested
					return "gp";
				case ShaderType.Hull:
#warning untested
					return "hp";
				case ShaderType.Domain:
#warning untested
					return "dp";

				default:
					throw new NotSupportedException($"ShaderType {_this} isn't supported");
			}
		}
	}
}
