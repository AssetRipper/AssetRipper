namespace AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;

public enum ShaderGpuProgramType53
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
	ConsoleVS = 25,
	ConsoleFS = 26,
	ConsoleHS = 27,
	ConsoleDS = 28,
	ConsoleGS = 29,
}

public static class ShaderGpuProgramType53Extensions
{
	public static ShaderGpuProgramType ToGpuProgramType(this ShaderGpuProgramType53 _this)
	{
		return _this switch
		{
			ShaderGpuProgramType53.Unknown => ShaderGpuProgramType.Unknown,
			ShaderGpuProgramType53.GLLegacy => ShaderGpuProgramType.GLLegacy,
			ShaderGpuProgramType53.GLES31AEP => ShaderGpuProgramType.GLES31AEP,
			ShaderGpuProgramType53.GLES31 => ShaderGpuProgramType.GLES31,
			ShaderGpuProgramType53.GLES3 => ShaderGpuProgramType.GLES3,
			ShaderGpuProgramType53.GLES => ShaderGpuProgramType.GLES,
			ShaderGpuProgramType53.GLCore32 => ShaderGpuProgramType.GLCore32,
			ShaderGpuProgramType53.GLCore41 => ShaderGpuProgramType.GLCore41,
			ShaderGpuProgramType53.GLCore43 => ShaderGpuProgramType.GLCore43,
			ShaderGpuProgramType53.DX9VertexSM20 => ShaderGpuProgramType.DX9VertexSM20,
			ShaderGpuProgramType53.DX9VertexSM30 => ShaderGpuProgramType.DX9VertexSM30,
			ShaderGpuProgramType53.DX9PixelSM20 => ShaderGpuProgramType.DX9PixelSM20,
			ShaderGpuProgramType53.DX9PixelSM30 => ShaderGpuProgramType.DX9PixelSM30,
			ShaderGpuProgramType53.DX10Level9Vertex => ShaderGpuProgramType.DX10Level9Vertex,
			ShaderGpuProgramType53.DX10Level9Pixel => ShaderGpuProgramType.DX10Level9Pixel,
			ShaderGpuProgramType53.DX11VertexSM40 => ShaderGpuProgramType.DX11VertexSM40,
			ShaderGpuProgramType53.DX11VertexSM50 => ShaderGpuProgramType.DX11VertexSM50,
			ShaderGpuProgramType53.DX11PixelSM40 => ShaderGpuProgramType.DX11PixelSM40,
			ShaderGpuProgramType53.DX11PixelSM50 => ShaderGpuProgramType.DX11PixelSM50,
			ShaderGpuProgramType53.DX11GeometrySM40 => ShaderGpuProgramType.DX11GeometrySM40,
			ShaderGpuProgramType53.DX11GeometrySM50 => ShaderGpuProgramType.DX11GeometrySM50,
			ShaderGpuProgramType53.DX11HullSM50 => ShaderGpuProgramType.DX11HullSM50,
			ShaderGpuProgramType53.DX11DomainSM50 => ShaderGpuProgramType.DX11DomainSM50,
			ShaderGpuProgramType53.MetalVS => ShaderGpuProgramType.MetalVS,
			ShaderGpuProgramType53.MetalFS => ShaderGpuProgramType.MetalFS,
			ShaderGpuProgramType53.ConsoleVS => ShaderGpuProgramType.ConsoleVS,
			ShaderGpuProgramType53.ConsoleFS => ShaderGpuProgramType.ConsoleFS,
			ShaderGpuProgramType53.ConsoleHS => ShaderGpuProgramType.ConsoleHS,
			ShaderGpuProgramType53.ConsoleDS => ShaderGpuProgramType.ConsoleDS,
			ShaderGpuProgramType53.ConsoleGS => ShaderGpuProgramType.ConsoleGS,
			_ => throw new Exception($"Unsupported gpu program type {_this}"),
		};
	}
}
