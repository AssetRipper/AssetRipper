using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.VersionUtilities;
using ShaderTextRestorer.Handlers;

namespace ShaderTextRestorer.Exporters.DirectX
{
	public static class DXShaderTextExtractor
	{
		public static bool TryGetShaderText(byte[] data, UnityVersion version, GPUPlatform gpuPlatform, out string disassemblyText)
		{
			int dataOffset = 0;
			if (DXDataHeader.HasHeader(gpuPlatform))
			{
				dataOffset = DXDataHeader.GetDataOffset(version, gpuPlatform);
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

		public static bool TryDecompileText(byte[] data, UnityVersion version, GPUPlatform gpuPlatform, out string decompiledText)
		{
			int dataOffset = 0;
			if (DXDataHeader.HasHeader(gpuPlatform))
			{
				dataOffset = DXDataHeader.GetDataOffset(version, gpuPlatform);
			}

			return DXDecompilerlyHandler.TryDecompile(data, dataOffset, out decompiledText);
		}
	}
}
