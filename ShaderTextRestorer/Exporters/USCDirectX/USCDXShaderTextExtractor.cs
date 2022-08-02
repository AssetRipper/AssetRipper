using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.VersionUtilities;
using ShaderLabConvert;
using ShaderTextRestorer.Exporters.DirectX;
using ShaderTextRestorer.Handlers;
using ShaderTextRestorer.ShaderBlob;

namespace ShaderTextRestorer.Exporters.USCDirectX
{
	public static class USCDXShaderTextExtractor
	{
		public static bool TryGetShaderText(byte[] data, UnityVersion version, GPUPlatform gpuPlatform, out string disassemblyText)
		{
			int dataOffset = 0;
			if (DXDataHeader.HasHeader(gpuPlatform))
			{
				dataOffset = DXDataHeader.GetDataOffset(version, gpuPlatform, data[0]);
			}

			if (DXDecompilerlyHandler.TryDisassemble(data, dataOffset, out disassemblyText))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool TryDecompileText(byte[] data, UnityVersion version, GPUPlatform gpuPlatform, ShaderSubProgram subProgram, out string decompiledText, out UShaderProgram uShaderProgram)
		{
			int dataOffset = 0;
			if (DXDataHeader.HasHeader(gpuPlatform))
			{
				dataOffset = DXDataHeader.GetDataOffset(version, gpuPlatform, data[0]);
			}

			return USCDecompilerHandler.TryDecompile(data, dataOffset, subProgram, out decompiledText, out uShaderProgram);
		}
	}
}
