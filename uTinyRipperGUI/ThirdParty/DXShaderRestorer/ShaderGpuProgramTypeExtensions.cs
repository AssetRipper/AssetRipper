using System;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	public static class ShaderGpuProgramTypeExtensions
	{
		public static DXProgramType ToDXProgramType(this ShaderGpuProgramType _this)
		{
			switch (_this)
			{
				case ShaderGpuProgramType.DX10Level9Pixel:
				case ShaderGpuProgramType.DX11PixelSM40:
				case ShaderGpuProgramType.DX11PixelSM50:
					return DXProgramType.PixelShader;

				case ShaderGpuProgramType.DX10Level9Vertex:
				case ShaderGpuProgramType.DX11VertexSM40:
				case ShaderGpuProgramType.DX11VertexSM50:
					return DXProgramType.VertexShader;

				case ShaderGpuProgramType.DX11GeometrySM40:
				case ShaderGpuProgramType.DX11GeometrySM50:
					return DXProgramType.GeometryShader;

				case ShaderGpuProgramType.DX11HullSM50:
					return DXProgramType.HullShader;

				case ShaderGpuProgramType.DX11DomainSM50:
					return DXProgramType.DomainShader;

				default:
					throw new Exception($"Unexpected program type {_this}");
			}
		}

		public static int GetMajorDXVersion(this ShaderGpuProgramType _this)
		{
			switch (_this)
			{
				case ShaderGpuProgramType.DX10Level9Vertex:
				case ShaderGpuProgramType.DX10Level9Pixel:
				case ShaderGpuProgramType.DX11PixelSM40:
				case ShaderGpuProgramType.DX11VertexSM40:
				case ShaderGpuProgramType.DX11GeometrySM40:
					return 4;

				case ShaderGpuProgramType.DX11PixelSM50:
				case ShaderGpuProgramType.DX11VertexSM50:
				case ShaderGpuProgramType.DX11GeometrySM50:
				case ShaderGpuProgramType.DX11HullSM50:
				case ShaderGpuProgramType.DX11DomainSM50:
					return 5;

				case ShaderGpuProgramType.RayTracing:
					return 6;

				default:
					throw new Exception($"Unexpected program type {_this}");
			}
		}
	}
}
