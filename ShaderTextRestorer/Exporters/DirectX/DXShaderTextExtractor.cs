using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.VersionUtilities;
using ShaderTextRestorer.Handlers;
using System.Diagnostics.CodeAnalysis;

namespace ShaderTextRestorer.Exporters.DirectX
{
	public static class DXShaderTextExtractor
	{
		public static bool TryGetShaderText(byte[] data, UnityVersion version, GPUPlatform gpuPlatform, [NotNullWhen(true)] out string? disassemblyText)
		{
			int dataOffset = GetDataOffset(data, version, gpuPlatform);
			return DXDecompilerlyHandler.TryDisassemble(data, dataOffset, out disassemblyText);
		}

		public static bool TryDecompileText(byte[] data, UnityVersion version, GPUPlatform gpuPlatform, [NotNullWhen(true)] out string? decompiledText)
		{
			int dataOffset = GetDataOffset(data, version, gpuPlatform);
			return DXDecompilerlyHandler.TryDecompile(data, dataOffset, out decompiledText);
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
