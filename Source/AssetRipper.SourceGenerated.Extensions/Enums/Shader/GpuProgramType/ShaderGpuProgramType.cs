using AssetRipper.IO.Files;
using System;

namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;

/// <remarks>
/// This is a native-only type. It has no managed equivalent.
/// </remarks>
public enum ShaderGpuProgramType
{
	Unknown = 0,
	GLLegacy = 1,
	GLES31AEP = 2,
	GLES31 = 3,
	GLES3 = 4,
	GLES = 5,
	GLCore32 = 6,
	GLCore41 = 7,
	GLCore43 = 8,
	DX9VertexSM20 = 9,
	DX9VertexSM30 = 10,
	DX9PixelSM20 = 11,
	DX9PixelSM30 = 12,
	DX10Level9Vertex = 13,
	DX10Level9Pixel = 14,
	DX11VertexSM40 = 15,
	DX11VertexSM50 = 16,
	DX11PixelSM40 = 17,
	DX11PixelSM50 = 18,
	DX11GeometrySM40 = 19,
	DX11GeometrySM50 = 20,
	DX11HullSM50 = 21,
	DX11DomainSM50 = 22,
	MetalVS = 23,
	MetalFS = 24,
	SPIRV = 25,
	// The alias 'Console = 26' has been removed to avoid ambiguity with ConsoleVS.
	ConsoleVS = 26,
	ConsoleFS = 27,
	ConsoleHS = 28,
	ConsoleDS = 29,
	ConsoleGS = 30,
	RayTracing = 31,
	PS5NGGC = 32
}

public static class ShaderGpuProgramTypeExtensions
{
	/// <summary>
	/// 5.5.0 and greater
	/// </summary>
	public static bool GpuProgramType55Relevant(UnityVersion version) => version.GreaterThanOrEquals(5, 5);

	public static bool IsGL(this ShaderGpuProgramType _this)
	{
		return _this switch
		{
			ShaderGpuProgramType.GLLegacy or
			ShaderGpuProgramType.GLCore32 or
			ShaderGpuProgramType.GLCore41 or
			ShaderGpuProgramType.GLCore43 or
			ShaderGpuProgramType.GLES or
			ShaderGpuProgramType.GLES3 or
			ShaderGpuProgramType.GLES31 or
			ShaderGpuProgramType.GLES31AEP => true,
			_ => false,
		};
	}

	public static bool IsMetal(this ShaderGpuProgramType _this)
	{
		return _this is ShaderGpuProgramType.MetalFS or ShaderGpuProgramType.MetalVS;
	}

	public static bool IsDX(this ShaderGpuProgramType _this)
	{
		// Enum values for DirectX are contiguous from 9 to 22.
		return _this >= ShaderGpuProgramType.DX9VertexSM20 && _this <= ShaderGpuProgramType.DX11DomainSM50;
	}

	public static bool IsDX9(this ShaderGpuProgramType _this)
	{
		// Enum values for DirectX 9 are contiguous from 9 to 12.
		return _this >= ShaderGpuProgramType.DX9VertexSM20 && _this <= ShaderGpuProgramType.DX9PixelSM30;
	}

	public static GPUPlatform ToGPUPlatform(this ShaderGpuProgramType _this, BuildTarget platform)
	{
		switch (_this)
		{
			case ShaderGpuProgramType.Unknown:
				return GPUPlatform.Unknown;

			case ShaderGpuProgramType.GLES:
				return GPUPlatform.Gles20;

			case ShaderGpuProgramType.GLES3:
			case ShaderGpuProgramType.GLES31:
			case ShaderGpuProgramType.GLES31AEP:
				return GPUPlatform.Gles3x;

			case ShaderGpuProgramType.GLCore32:
			case ShaderGpuProgramType.GLCore41:
			case ShaderGpuProgramType.GLCore43:
				return GPUPlatform.GlCore;

			case ShaderGpuProgramType.GLLegacy:
				return GPUPlatform.OpenGL;

			case ShaderGpuProgramType.DX9VertexSM20:
			case ShaderGpuProgramType.DX9VertexSM30:
			case ShaderGpuProgramType.DX9PixelSM20:
			case ShaderGpuProgramType.DX9PixelSM30:
				return GPUPlatform.D3D9;

			case ShaderGpuProgramType.DX10Level9Pixel:
			case ShaderGpuProgramType.DX10Level9Vertex:
				return GPUPlatform.D3D11_9x;

			case ShaderGpuProgramType.DX11VertexSM40:
			case ShaderGpuProgramType.DX11VertexSM50:
			case ShaderGpuProgramType.DX11PixelSM40:
			case ShaderGpuProgramType.DX11PixelSM50:
			case ShaderGpuProgramType.DX11GeometrySM40:
			case ShaderGpuProgramType.DX11GeometrySM50:
			case ShaderGpuProgramType.DX11HullSM50:
			case ShaderGpuProgramType.DX11DomainSM50:
				return GPUPlatform.D3D11;

			case ShaderGpuProgramType.MetalVS:
			case ShaderGpuProgramType.MetalFS:
				return GPUPlatform.Metal;

			case ShaderGpuProgramType.SPIRV:
				return GPUPlatform.Vulkan;
				
			case ShaderGpuProgramType.PS5NGGC:
				return GPUPlatform.PS5;

			case ShaderGpuProgramType.RayTracing:
				// RayTracing is a pipeline type, not a specific hardware platform.
				return GPUPlatform.Unknown;

			case ShaderGpuProgramType.ConsoleVS:
			case ShaderGpuProgramType.ConsoleFS:
			case ShaderGpuProgramType.ConsoleHS:
			case ShaderGpuProgramType.ConsoleDS:
			case ShaderGpuProgramType.ConsoleGS:
				return platform switch
				{
					BuildTarget.PS3 => GPUPlatform.PS3,
					BuildTarget.PS4 => GPUPlatform.PS4,
					BuildTarget.PS5 => GPUPlatform.PS5,
					BuildTarget.PSM => GPUPlatform.PSM,
					BuildTarget.PSP2 => GPUPlatform.Vita,
					BuildTarget.Xbox360 => GPUPlatform.Xbox360,
					// We cannot reliably distinguish between D3D11 and D3D12 for XboxOne from shader data alone.
					// Defaulting to the base XboxOne (D3D11) is the safest and most consistent approach.
					BuildTarget.XboxOne => GPUPlatform.XboxOne,
					BuildTarget.WiiU => GPUPlatform.WiiU,
					BuildTarget.N3DS => GPUPlatform.N3DS,
					BuildTarget.GoogleNaCl => GPUPlatform.GlesDesktop,
					BuildTarget.Flash => GPUPlatform.Flash,
					BuildTarget.Switch => GPUPlatform.Switch,
					_ => throw new NotSupportedException($"Unsupported console platform {platform}"),
				};

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
				// Serialization only works for Unity 5.4+, where this is always "!!GLSL".
				return "!!GLSL";

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

			case ShaderGpuProgramType.RayTracing:
				return "raytracing";

			case ShaderGpuProgramType.PS5NGGC:
			case ShaderGpuProgramType.ConsoleVS:
			case ShaderGpuProgramType.ConsoleFS:
			case ShaderGpuProgramType.ConsoleGS:
			case ShaderGpuProgramType.ConsoleHS:
			case ShaderGpuProgramType.ConsoleDS:
				return platform switch
				{
					BuildTarget.Flash => type is ShaderType.Vertex ? "agal_vs" : "agal_ps",
					BuildTarget.PS4 => _this switch
					{
						ShaderGpuProgramType.ConsoleVS => "pssl_vs",
						ShaderGpuProgramType.ConsoleFS => "pssl_ps",
						ShaderGpuProgramType.ConsoleGS => "pssl_gs",
						_ => throw new NotSupportedException($"Unsupported gpu program type {_this} for PS4"),
					},
					BuildTarget.PS5 => _this switch
					{
						ShaderGpuProgramType.PS5NGGC => "ps5nggc", // Map to PS5 native next-gen geometry compiler
						_ => throw new NotSupportedException($"Unsupported gpu program type {_this} for PS5"),
					},
					BuildTarget.Switch => type switch
					{
						ShaderType.Vertex => "nssl_vs",
						ShaderType.Fragment => "nssl_ps",
						ShaderType.Geometry => "nssl_gs",
						ShaderType.Hull => "nssl_hs",
						_ => throw new NotSupportedException($"Unsupported shader type {type} for Switch"),
					},
					_ => throw new NotSupportedException($"Unsupported platform {platform} for console gpu program type {_this}"),
				};

			default:
				throw new NotSupportedException($"Unsupported gpu program type {_this}[{platform}, {type}]");
		}
	}

	public static int ToShaderModelVersion(this ShaderGpuProgramType _this)
	{
		return _this switch
		{
			ShaderGpuProgramType.DX9VertexSM20 or ShaderGpuProgramType.DX9PixelSM20 => 20,
			ShaderGpuProgramType.DX9VertexSM30 or ShaderGpuProgramType.DX9PixelSM30 => 30,
			ShaderGpuProgramType.DX10Level9Vertex or ShaderGpuProgramType.DX10Level9Pixel => 40,
			ShaderGpuProgramType.DX11VertexSM40 or ShaderGpuProgramType.DX11PixelSM40 or ShaderGpuProgramType.DX11GeometrySM40 => 40,
			ShaderGpuProgramType.DX11VertexSM50 or ShaderGpuProgramType.DX11PixelSM50 or ShaderGpuProgramType.DX11GeometrySM50 or
			ShaderGpuProgramType.DX11HullSM50 or ShaderGpuProgramType.DX11DomainSM50 => 50,
			_ => default,
		};
	}
}
