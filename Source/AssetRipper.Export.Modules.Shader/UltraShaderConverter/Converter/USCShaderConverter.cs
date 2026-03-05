using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.DirectX;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;

namespace AssetRipper.Export.Modules.Shaders.UltraShaderConverter.Converter;

public class USCShaderConverter
{
	public DirectXCompiledShader? DxShader { get; set; }
	public UShaderProgram? ShaderProgram { get; set; }

	[MemberNotNull(nameof(DxShader))]
	public void LoadDirectXCompiledShader(Stream data)
	{
		DxShader = new DirectXCompiledShader(data);
	}

	[MemberNotNull(nameof(ShaderProgram))]
	public void ConvertShaderToUShaderProgram()
	{
		if (DxShader == null)
		{
			throw new Exception($"You need to call {nameof(LoadDirectXCompiledShader)} first!");
		}

		DirectXProgramToUSIL dx2UsilConverter = new DirectXProgramToUSIL(DxShader);
		dx2UsilConverter.Convert();

		ShaderProgram = dx2UsilConverter.shader;
	}

	public void ApplyMetadataToProgram(ShaderSubProgram subProgram, UnityVersion version)
	{
		if (ShaderProgram == null)
		{
			throw new Exception($"You need to call {nameof(ConvertShaderToUShaderProgram)} first!");
		}

		ShaderGpuProgramType shaderProgramType = subProgram.GetProgramType(version);

		bool isVertex = shaderProgramType == ShaderGpuProgramType.DX11VertexSM40 || shaderProgramType == ShaderGpuProgramType.DX11VertexSM50;
		bool isFragment = shaderProgramType == ShaderGpuProgramType.DX11PixelSM40 || shaderProgramType == ShaderGpuProgramType.DX11PixelSM50;

		if (!isVertex && !isFragment)
		{
			throw new NotSupportedException("Only vertex and fragment shaders are supported at the moment!");
		}

		ShaderProgram.shaderFunctionType = isVertex ? UShaderFunctionType.Vertex : UShaderFunctionType.Fragment;

		USILOptimizerApplier.Apply(ShaderProgram, subProgram);
	}

	public string CovnertUShaderProgramToHLSL(int depth)
	{
		if (ShaderProgram == null)
		{
			throw new Exception($"You need to call {nameof(ConvertShaderToUShaderProgram)} first!");
		}

		UShaderFunctionToHLSL hlslConverter = new UShaderFunctionToHLSL(ShaderProgram);
		return hlslConverter.Convert(depth);
	}
}
