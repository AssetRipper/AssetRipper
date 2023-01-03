using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.VersionUtilities;
using ShaderLabConvert;
using ShaderTextRestorer.Exporters.DirectX;
using ShaderTextRestorer.Handlers;
using ShaderTextRestorer.ShaderBlob;
using System.Diagnostics.CodeAnalysis;

namespace ShaderTextRestorer.Exporters.USCDirectX
{
	public static class USCDXShaderTextExtractor
	{
		public static bool TryGetShaderText(byte[] data, UnityVersion version, GPUPlatform gpuPlatform, [NotNullWhen(true)] out string? disassemblyText)
		{
			int dataOffset = GetDataOffset(data, version, gpuPlatform);
			return DXDecompilerlyHandler.TryDisassemble(data, dataOffset, out disassemblyText);
		}

		public static bool TryDecompileText(byte[] data, UnityVersion version, GPUPlatform gpuPlatform, ShaderSubProgram subProgram, [NotNullWhen(true)] out string? decompiledText, out UShaderProgram? uShaderProgram)
		{
			int dataOffset = GetDataOffset(data, version, gpuPlatform);
			return USCDecompilerHandler.TryDecompile(data, dataOffset, subProgram, out decompiledText, out uShaderProgram);
		}

		private static int GetDataOffset(byte[] data, UnityVersion version, GPUPlatform gpuPlatform)
		{
			if (DXDataHeader.HasHeader(gpuPlatform))
			{
				return DXDataHeader.GetDataOffset(version, gpuPlatform, data[0]);
			}
			else
			{
				return 0;
			}
		}
	}
}
