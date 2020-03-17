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
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		RayTracing	= 6,

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
					return "gp";
				case ShaderType.Hull:
					return "hp";
				case ShaderType.Domain:
					return "dp";
				case ShaderType.RayTracing:
					return "rtp";

				default:
					throw new NotSupportedException($"ShaderType {_this} isn't supported");
			}
		}

		public static int ToProgramMask(this ShaderType _this)
		{
			return 1 << (int)_this;
		}
	}
}
