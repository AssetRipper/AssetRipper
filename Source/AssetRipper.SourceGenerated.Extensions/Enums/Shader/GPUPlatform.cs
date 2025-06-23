namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader;

/// <summary>
/// Graphic API. Also called ShaderCompilerPlatform<br/>
/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Graphics/ShaderCompilerData.cs"/>
/// </summary>
public enum GPUPlatform
{
	/// <summary>
	/// For inner use only
	/// </summary>
	unknown = -1,
	/// <summary>
	/// For non initialized variable.
	/// </summary>
	none = 0,
	openGL = 0,
	d3d9 = 1,
	xbox360 = 2,
	ps3 = 3,
	/// <summary>
	/// Direct3D 11 (FL10.0 and up) and Direct3D 12, compiled with MS D3DCompiler
	/// </summary>
	d3d11 = 4,
	/// <summary>
	/// OpenGL ES 2.0 / WebGL 1.0, compiled with MS D3DCompiler + HLSLcc
	/// </summary>
	gles = 5,
	/// <summary>
	/// OpenGL ES 3.0+ / WebGL 2.0, compiled with MS D3DCompiler + HLSLcc
	/// </summary>
	glesdesktop = 6,
	flash = 7,
	d3d11_9x = 8,
	gles3 = 9,
	psp2 = 10,
	/// <summary>
	/// Sony PS4
	/// </summary>
	ps4 = 11,
	/// <summary>
	/// MS XboxOne
	/// </summary>
	xboxone = 12,
	psm = 13,
	/// <summary>
	/// Apple Metal, compiled with MS D3DCompiler + HLSLcc
	/// </summary>
	metal = 14,
	/// <summary>
	/// Desktop OpenGL 3+, compiled with MS D3DCompiler + HLSLcc
	/// </summary>
	glcore = 15,
	n3ds = 16,
	wiiu = 17,
	/// <summary>
	/// Vulkan SPIR-V, compiled with MS D3DCompiler + HLSLcc
	/// </summary>
	vulkan = 18,
	/// <summary>
	/// Nintendo Switch (NVN)
	/// </summary>
	Switch = 19,
	/// <summary>
	/// Xbox One D3D12
	/// </summary>
	xboxone_d3d12 = 20,
	/// <summary>
	/// Game Core
	/// </summary>
	GameCoreXboxOne = 21,
	GameCoreScarlett = 22,
	/// <summary>
	/// PS5
	/// </summary>
	PS5 = 23,
	/// <summary>
	/// PS5 NGGC
	/// </summary>
	PS5NGGC = 24,
}
