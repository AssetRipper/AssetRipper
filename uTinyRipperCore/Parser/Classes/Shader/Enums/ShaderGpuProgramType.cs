using System;

namespace uTinyRipper.Classes.Shaders
{
	public enum ShaderGpuProgramType
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
		Console				= 25,
		PSVertex			= 26,
		PSPixel				= 27,
	}

	public static class ShaderGpuProgramTypeExtensions
	{
		public static bool IsGL(this ShaderGpuProgramType _this)
		{
			switch (_this)
			{
				case ShaderGpuProgramType.GLLegacy:
				case ShaderGpuProgramType.GLCore32:
				case ShaderGpuProgramType.GLCore41:
				case ShaderGpuProgramType.GLCore43:
				case ShaderGpuProgramType.GLES:
				case ShaderGpuProgramType.GLES3:
				case ShaderGpuProgramType.GLES31:
				case ShaderGpuProgramType.GLES31AEP:
					return true;

				default:
					return false;
			}
		}

		public static bool IsMetal(this ShaderGpuProgramType _this)
		{
			switch (_this)
			{
				case ShaderGpuProgramType.MetalFS:
				case ShaderGpuProgramType.MetalVS:
					return true;

				default:
					return false;
			}
		}

		public static bool IsDX(this ShaderGpuProgramType _this)
		{
			switch (_this)
			{
				case ShaderGpuProgramType.DX9PixelSM20:
				case ShaderGpuProgramType.DX9PixelSM30:
				case ShaderGpuProgramType.DX9VertexSM20:
				case ShaderGpuProgramType.DX9VertexSM30:
				case ShaderGpuProgramType.DX10Level9Pixel:
				case ShaderGpuProgramType.DX10Level9Vertex:
				case ShaderGpuProgramType.DX11DomainSM50:
				case ShaderGpuProgramType.DX11GeometrySM40:
				case ShaderGpuProgramType.DX11GeometrySM50:
				case ShaderGpuProgramType.DX11HullSM50:
				case ShaderGpuProgramType.DX11PixelSM40:
				case ShaderGpuProgramType.DX11PixelSM50:
				case ShaderGpuProgramType.DX11VertexSM40:
				case ShaderGpuProgramType.DX11VertexSM50:
					return true;

				default:
					return false;
			}
		}

		public static GPUPlatform ToGPUPlatform(this ShaderGpuProgramType _this, Platform platform)
		{
			switch(_this)
			{
				case ShaderGpuProgramType.GLES:
					return GPUPlatform.gles;

				case ShaderGpuProgramType.GLES3:
				case ShaderGpuProgramType.GLES31:
				case ShaderGpuProgramType.GLES31AEP:
					return GPUPlatform.gles3;

				case ShaderGpuProgramType.GLCore32:
				case ShaderGpuProgramType.GLCore41:
				case ShaderGpuProgramType.GLCore43:
					return GPUPlatform.glcore;

				case ShaderGpuProgramType.GLLegacy:
					return GPUPlatform.openGL;

				case ShaderGpuProgramType.DX9VertexSM20:
				case ShaderGpuProgramType.DX9VertexSM30:
				case ShaderGpuProgramType.DX9PixelSM20:
				case ShaderGpuProgramType.DX9PixelSM30:
					return GPUPlatform.d3d9;

				case ShaderGpuProgramType.DX10Level9Pixel:
				case ShaderGpuProgramType.DX10Level9Vertex:
					return GPUPlatform.d3d11_9x;

				case ShaderGpuProgramType.DX11VertexSM40:
				case ShaderGpuProgramType.DX11VertexSM50:
				case ShaderGpuProgramType.DX11PixelSM40:
				case ShaderGpuProgramType.DX11PixelSM50:
				case ShaderGpuProgramType.DX11GeometrySM40:
				case ShaderGpuProgramType.DX11GeometrySM50:
				case ShaderGpuProgramType.DX11HullSM50:
				case ShaderGpuProgramType.DX11DomainSM50:
					return GPUPlatform.d3d11;

				case ShaderGpuProgramType.MetalVS:
				case ShaderGpuProgramType.MetalFS:
					return GPUPlatform.metal;

				case ShaderGpuProgramType.Console:
					switch(platform)
					{
						case Platform.PS3:
							return GPUPlatform.ps3;
						case Platform.PS4:
							return GPUPlatform.ps4;
						case Platform.PSM:
							return GPUPlatform.psm;
						case Platform.PSP2:
							return GPUPlatform.psp2;

						case Platform.XBox360:
							return GPUPlatform.xbox360;
						case Platform.XboxOne:
							return GPUPlatform.xboxone;
#warning TODO:
							//return GPUPlatform.xboxone_d3d12;

						case Platform.WiiU:
							return GPUPlatform.wiiu;

						case Platform.N3DS:
							return GPUPlatform.n3ds;

						case Platform.GoogleNaCl:
							return GPUPlatform.glesdesktop;

						case Platform.Flash:
							return GPUPlatform.flash;

						case Platform.StandaloneWinPlayer:
						case Platform.StandaloneWin64Player:
						case Platform.Android:
							return GPUPlatform.vulkan;

						case Platform.Switch:
							return GPUPlatform.@switch;

						default:
							throw new NotSupportedException($"Unsupported console platform {platform}");
					}

				case ShaderGpuProgramType.PSVertex:
				case ShaderGpuProgramType.PSPixel:
					return GPUPlatform.ps4;

				default:
					throw new NotSupportedException($"Unsupported gpu program type {_this}");
			}
		}
	}
}
