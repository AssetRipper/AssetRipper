using AssetRipper.Core.Parser.Files;
using System;


namespace AssetRipper.Core.Classes.Shader.Enums.GpuProgramType
{
	public enum ShaderGpuProgramType
	{
		Unknown,
		GLLegacy,
		GLES31AEP,
		GLES31,
		GLES3,
		GLES,
		GLCore32,
		GLCore41,
		GLCore43,
		DX9VertexSM20,
		DX9VertexSM30,
		DX9PixelSM20,
		DX9PixelSM30,
		DX10Level9Vertex,
		DX10Level9Pixel,
		DX11VertexSM40,
		DX11VertexSM50,
		DX11PixelSM40,
		DX11PixelSM50,
		DX11GeometrySM40,
		DX11GeometrySM50,
		DX11HullSM50,
		DX11DomainSM50,
		MetalVS,
		MetalFS,
		SPIRV,

#warning Console is omitted in AssetStudio
		Console,

		ConsoleVS,
		ConsoleFS,
		ConsoleHS,
		ConsoleDS,
		ConsoleGS,
		RayTracing,
	}

	public static class ShaderGpuProgramTypeExtensions
	{
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool GpuProgramType55Relevant(UnityVersion version) => version.IsGreaterEqual(5, 5);

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
			return _this switch
			{
				ShaderGpuProgramType.MetalFS or ShaderGpuProgramType.MetalVS => true,
				_ => false,
			};
		}

		public static bool IsDX(this ShaderGpuProgramType _this)
		{
			switch(_this)
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

		public static bool IsDX9(this ShaderGpuProgramType _this)
		{
			switch (_this)
			{
				case ShaderGpuProgramType.DX9PixelSM20:
				case ShaderGpuProgramType.DX9PixelSM30:
				case ShaderGpuProgramType.DX9VertexSM20:
				case ShaderGpuProgramType.DX9VertexSM30:
					return true;

				default:
					return false;
			}
		}

		public static GPUPlatform ToGPUPlatform(this ShaderGpuProgramType _this, BuildTarget platform)
		{
			switch (_this)
			{
				case ShaderGpuProgramType.Unknown:
					return GPUPlatform.unknown;

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

				case ShaderGpuProgramType.SPIRV:
					return GPUPlatform.vulkan;

				case ShaderGpuProgramType.Console:
				case ShaderGpuProgramType.ConsoleVS:
				case ShaderGpuProgramType.ConsoleFS:
				case ShaderGpuProgramType.ConsoleHS:
				case ShaderGpuProgramType.ConsoleDS:
				case ShaderGpuProgramType.ConsoleGS:
					switch (platform)
					{
						case BuildTarget.PS3:
							return GPUPlatform.ps3;
						case BuildTarget.PS4:
							return GPUPlatform.ps4;
						case BuildTarget.PSM:
							return GPUPlatform.psm;
						case BuildTarget.PSP2:
							return GPUPlatform.psp2;

						case BuildTarget.XBox360:
							return GPUPlatform.xbox360;
						case BuildTarget.XboxOne:
							return GPUPlatform.xboxone;
#warning		 TODO:
						//return GPUPlatform.xboxone_d3d12;

						case BuildTarget.WiiU:
							return GPUPlatform.wiiu;

						case BuildTarget.N3DS:
							return GPUPlatform.n3ds;

						case BuildTarget.GoogleNaCl:
							return GPUPlatform.glesdesktop;

						case BuildTarget.Flash:
							return GPUPlatform.flash;

						case BuildTarget.Switch:
							return GPUPlatform.Switch;

						default:
							throw new NotSupportedException($"Unsupported console platform {platform}");
					}

				default:
					throw new NotSupportedException($"Unsupported gpu program type {_this}");
			}
		}

		public static string ToProgramDataKeyword(this ShaderGpuProgramType _this, BuildTarget platform, ShaderType type)
		{
			switch (_this)
			{
				case ShaderGpuProgramType.Unknown:
					return nameof(ShaderGpuProgramType.Unknown);

				case ShaderGpuProgramType.GLES:
					return "!!GLES";
				case ShaderGpuProgramType.GLES3:
				case ShaderGpuProgramType.GLES31:
				case ShaderGpuProgramType.GLES31AEP:
					return "!!GLES3";

				case ShaderGpuProgramType.GLCore32:
					return "!!GL3x";
				case ShaderGpuProgramType.GLCore41:
				case ShaderGpuProgramType.GLCore43:
					return "!!GL4x";

				case ShaderGpuProgramType.GLLegacy:
					{
						// for ver < 5.0
						/*if (type == ShaderType.Vertex)
						{
							return "!!ARBvp1.0";
						}
						else if (type == ShaderType.Fragment)
						{
							return "!!ARBfp1.0";
						}*/
						// but since serialization work only for >= 5.4 always return
						return "!!GLSL"; // v1.20
					}

				case ShaderGpuProgramType.DX9VertexSM20:
					return "vs_2_0";
				case ShaderGpuProgramType.DX9VertexSM30:
					return "vs_3_0";
				case ShaderGpuProgramType.DX9PixelSM20:
					return "ps_2_0";
				case ShaderGpuProgramType.DX9PixelSM30:
					return "ps_3_0";

				case ShaderGpuProgramType.DX10Level9Vertex:
					return "vs_4_0_level_9_1";
				case ShaderGpuProgramType.DX10Level9Pixel:
					return "ps_4_0_level_9_1";

				case ShaderGpuProgramType.DX11VertexSM40:
					return "vs_4_0";
				case ShaderGpuProgramType.DX11VertexSM50:
					return "vs_5_0";
				case ShaderGpuProgramType.DX11PixelSM40:
					return "ps_4_0";
				case ShaderGpuProgramType.DX11PixelSM50:
					return "ps_5_0";
				case ShaderGpuProgramType.DX11GeometrySM40:
					return "gs_4_0";
				case ShaderGpuProgramType.DX11GeometrySM50:
					return "gs_5_0";
				case ShaderGpuProgramType.DX11HullSM50:
					return "hs_5_0";
				case ShaderGpuProgramType.DX11DomainSM50:
					return "ds_5_0";

				case ShaderGpuProgramType.MetalVS:
					return "metal_vs";
				case ShaderGpuProgramType.MetalFS:
					return "metal_fs";

				case ShaderGpuProgramType.SPIRV:
					return "spirv";
			}

			switch (platform)
			{
				case BuildTarget.Flash:
					{
						if (_this == ShaderGpuProgramType.Console)
						{
							switch (type)
							{
								case ShaderType.Vertex:
									return "agal_vs";
								case ShaderType.Fragment:
									return "agal_ps";
							}
						}
					}
					break;

				case BuildTarget.PS4:
					{
						switch (_this)
						{
							case ShaderGpuProgramType.Console:
								{
									switch (type)
									{
										case ShaderType.Vertex:
											return "pssl_vs";
										case ShaderType.Fragment:
											return "pssl_ps";
										case ShaderType.Geometry:
											return "pssl_gs";
									}
								}
								break;

							case ShaderGpuProgramType.ConsoleVS:
								return "pssl_vs";
							case ShaderGpuProgramType.ConsoleFS:
								return "pssl_ps";
							case ShaderGpuProgramType.ConsoleGS:
								return "pssl_gs";
						}
					}
					break;

				case BuildTarget.Switch:
					{
						if (_this == ShaderGpuProgramType.Console)
						{
							switch (type)
							{
								case ShaderType.Vertex:
									return "nssl_vs";
								case ShaderType.Fragment:
									return "nssl_ps";
								case ShaderType.Geometry:
									return "nssl_gs";
								case ShaderType.Hull:
									return "nssl_hs";
							}
						}
					}
					break;
			}

			throw new NotSupportedException($"Unsupported gpu program type {_this} [{platform}, {type}]");
		}
	}
}
