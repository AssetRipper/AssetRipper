using System;

namespace uTinyRipper.Classes.Shaders
{
	public enum ShaderGpuProgramType55
	{
		Unknown				= 0,
		GLLegacy			= 1,
		GLES31AEP			= 2,
		GLES31				= 3,
		GLES3				= 4,
		GLES				= 5,
		GLCore32			= 6,
		GLCore41			= 7,
		GLCore43			= 8,
		DX9VertexSM20		= 9,
		DX9VertexSM30		= 10,
		DX9PixelSM20		= 11,
		DX9PixelSM30		= 12,
		DX10Level9Vertex	= 13,
		DX10Level9Pixel		= 14,
		DX11VertexSM40		= 15,
		DX11VertexSM50		= 16,
		DX11PixelSM40		= 17,
		DX11PixelSM50		= 18,
		DX11GeometrySM40	= 19,
		DX11GeometrySM50	= 20,
		DX11HullSM50		= 21,
		DX11DomainSM50		= 22,
		MetalVS				= 23,
		MetalFS				= 24,
		Spirv				= 25,
		Console				= 26,
		//ConsoleVertex		= 26,
		//ConsolePixel		= 27,
		//Console3			= 28,
		//Console4			= 29,
		//ConsoleGeometry	= 30,
		DX12RayTracingSM63	= 31,
	}

	public static class ShaderGpuProgramType55Extensions
	{
		public static ShaderGpuProgramType ToGpuProgramType(this ShaderGpuProgramType55 _this)
		{
			switch (_this)
			{
				case ShaderGpuProgramType55.Unknown:
					return ShaderGpuProgramType.Unknown;
				case ShaderGpuProgramType55.GLLegacy:
					return ShaderGpuProgramType.GLLegacy;
				case ShaderGpuProgramType55.GLES31AEP:
					return ShaderGpuProgramType.GLES31AEP;
				case ShaderGpuProgramType55.GLES31:
					return ShaderGpuProgramType.GLES31;
				case ShaderGpuProgramType55.GLES3:
					return ShaderGpuProgramType.GLES3;
				case ShaderGpuProgramType55.GLES:
					return ShaderGpuProgramType.GLES;
				case ShaderGpuProgramType55.GLCore32:
					return ShaderGpuProgramType.GLCore32;
				case ShaderGpuProgramType55.GLCore41:
					return ShaderGpuProgramType.GLCore41;
				case ShaderGpuProgramType55.GLCore43:
					return ShaderGpuProgramType.GLCore43;
				case ShaderGpuProgramType55.DX9VertexSM20:
					return ShaderGpuProgramType.DX9VertexSM20;
				case ShaderGpuProgramType55.DX9VertexSM30:
					return ShaderGpuProgramType.DX9VertexSM30;
				case ShaderGpuProgramType55.DX9PixelSM20:
					return ShaderGpuProgramType.DX9PixelSM20;
				case ShaderGpuProgramType55.DX9PixelSM30:
					return ShaderGpuProgramType.DX9PixelSM30;
				case ShaderGpuProgramType55.DX10Level9Vertex:
					return ShaderGpuProgramType.DX10Level9Vertex;
				case ShaderGpuProgramType55.DX10Level9Pixel:
					return ShaderGpuProgramType.DX10Level9Pixel;
				case ShaderGpuProgramType55.DX11VertexSM40:
					return ShaderGpuProgramType.DX11VertexSM40;
				case ShaderGpuProgramType55.DX11VertexSM50:
					return ShaderGpuProgramType.DX11VertexSM50;
				case ShaderGpuProgramType55.DX11PixelSM40:
					return ShaderGpuProgramType.DX11PixelSM40;
				case ShaderGpuProgramType55.DX11PixelSM50:
					return ShaderGpuProgramType.DX11PixelSM50;
				case ShaderGpuProgramType55.DX11GeometrySM40:
					return ShaderGpuProgramType.DX11GeometrySM40;
				case ShaderGpuProgramType55.DX11GeometrySM50:
					return ShaderGpuProgramType.DX11GeometrySM50;
				case ShaderGpuProgramType55.DX11HullSM50:
					return ShaderGpuProgramType.DX11HullSM50;
				case ShaderGpuProgramType55.DX11DomainSM50:
					return ShaderGpuProgramType.DX11DomainSM50;
				case ShaderGpuProgramType55.MetalVS:
					return ShaderGpuProgramType.MetalVS;
				case ShaderGpuProgramType55.MetalFS:
					return ShaderGpuProgramType.MetalFS;
				case ShaderGpuProgramType55.Spirv:
					return ShaderGpuProgramType.Spirv;
				case ShaderGpuProgramType55.Console:
					return ShaderGpuProgramType.Console;
				case ShaderGpuProgramType55.DX12RayTracingSM63:
					return ShaderGpuProgramType.DX12RayTracingSM63;

				default:
					throw new Exception($"Unsupported gpu program type {_this}");
			}
		}
	}
}
