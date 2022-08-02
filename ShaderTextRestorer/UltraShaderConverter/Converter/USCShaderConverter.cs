using AssetRipper.Core.Classes.Shader.Enums.GpuProgramType;
using AssetRipper.VersionUtilities;
using DirectXDisassembler;
using ShaderLabConvert;
using ShaderTextRestorer.ShaderBlob;
using System;
using System.IO;

namespace UltraShaderConverter
{
	public class USCShaderConverter
	{
		public DirectXCompiledShader DxShader { get; set; }
		public UShaderProgram ShaderProgram { get; set; }

		public void LoadDirectXCompiledShader(Stream data)
		{
			DxShader = new DirectXCompiledShader(data);
		}

		public void ConvertShaderToUShaderProgram()
		{
			if (DxShader == null)
			{
				throw new Exception("You need to call LoadDirectXCopmiledShader first!");
			}

			DirectXProgramToUSIL dx2UsilConverter = new DirectXProgramToUSIL(DxShader);
			dx2UsilConverter.Convert();

			ShaderProgram = dx2UsilConverter.shader;
		}

		public void ApplyMetadataToProgram(ShaderSubProgram subProgram, UnityVersion version)
		{
			if (ShaderProgram == null)
			{
				throw new Exception("You need to call ConvertShaderToUShaderProgram first!");
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
			UShaderFunctionToHLSL hlslConverter = new UShaderFunctionToHLSL(ShaderProgram);
			return hlslConverter.Convert(depth);
		}
	}
}
