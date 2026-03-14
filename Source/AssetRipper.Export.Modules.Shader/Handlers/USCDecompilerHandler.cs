using AssetRipper.Export.Modules.Shaders.ShaderBlob;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.DirectXDisassembler;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.DirectX;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.UShader.Function;
using AssetRipper.Export.Modules.Shaders.UltraShaderConverter.USIL;
using AssetRipper.Import.Logging;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;
using DXDecompiler.Util;
using System.Buffers.Binary;

namespace AssetRipper.Export.Modules.Shaders.Handlers;

public static class USCDecompilerHandler
{
	public static bool TryDecompile(byte[] data, int offset, ShaderSubProgram subProgram, [NotNullWhen(true)] out string? decompiledText, out UShaderProgram? uShaderProgram) =>
		TryDecompile(GetRelevantData(data, offset), subProgram, out decompiledText, out uShaderProgram);
	public static bool TryDecompile(byte[] data, ShaderSubProgram subProgram, [NotNullWhen(true)] out string? decompiledText, out UShaderProgram? uShaderProgram)
	{
		if (data == null)
		{
			throw new ArgumentNullException(nameof(data));
		}

		if (data.Length == 0)
		{
			throw new ArgumentException("inputData cannot have zero length");
		}

		uShaderProgram = null;

		try
		{
			DXProgramType programType = GetProgramType(data);
			switch (programType)
			{
				case DXProgramType.DXBC:

					DirectXCompiledShader dxShader = new DirectXCompiledShader(new MemoryStream(data));

					DirectXProgramToUSIL dx2UsilConverter = new DirectXProgramToUSIL(dxShader);
					dx2UsilConverter.Convert();

					// TODO: pass in real version, although 5.5- isn't supported anyway
					ShaderGpuProgramType shaderProgramType = subProgram.GetProgramType(new UnityVersion(5, 5));
					bool isVertex = shaderProgramType == ShaderGpuProgramType.DX11VertexSM40 || shaderProgramType == ShaderGpuProgramType.DX11VertexSM50;
					dx2UsilConverter.shader.shaderFunctionType = isVertex ? UShaderFunctionType.Vertex : UShaderFunctionType.Fragment;

					UShaderProgram uProg = dx2UsilConverter.shader;

					USILOptimizerApplier.Apply(uProg, subProgram);

					UShaderFunctionToHLSL hlslConverter = new UShaderFunctionToHLSL(uProg);

					decompiledText = hlslConverter.Convert(0);

					Logger.Info(LogCategory.Export, "USC successfully decompiled a DXBC shader");
					uShaderProgram = uProg;
					return !string.IsNullOrEmpty(decompiledText);
				default:
					throw new NotSupportedException("Only DX11 shaders can be decompiled at this time.");
			}
		}
		catch (Exception ex)
		{
			Logger.Info(LogCategory.Export, "USC threw an exception while attempting to decompile a shader");
			Logger.Info(LogCategory.Export, ex.ToString());
		}

		decompiledText = null;
		return false;
	}

	private static DXProgramType GetProgramType(ReadOnlySpan<byte> data)
	{
		if (data.Length < 4)
		{
			return DXProgramType.Unknown;
		}
		uint dxbcHeader = BinaryPrimitives.ReadUInt32LittleEndian(data);
		if (dxbcHeader == "DXBC".ToFourCc())
		{
			return DXProgramType.DXBC;
		}
		if (dxbcHeader == 0xFEFF2001)
		{
			return DXProgramType.DXBC;
		}
		DXDecompiler.DX9Shader.ShaderType dx9ShaderType = (DXDecompiler.DX9Shader.ShaderType)BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
		if (dx9ShaderType == DXDecompiler.DX9Shader.ShaderType.Vertex ||
			dx9ShaderType == DXDecompiler.DX9Shader.ShaderType.Pixel ||
			dx9ShaderType == DXDecompiler.DX9Shader.ShaderType.Effect)
		{
			return DXProgramType.DX9;
		}
		return DXProgramType.Unknown;
	}

	private static byte[] GetRelevantData(ReadOnlySpan<byte> bytes, int offset)
	{
		if (offset < 0 || offset > bytes.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(offset));
		}

		return bytes[offset..].ToArray();
	}

	private enum DXProgramType
	{
		Unknown,
		DX9,
		DXBC
	}
}
