using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.Parser.Files;
using ShaderTextRestorer.Handlers;
using System;

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
			else if (D3DHandler.IsD3DAvailable())
			{
				uint fourCC = BitConverter.ToUInt32(data, dataOffset);
				if (!D3DHandler.IsCompatible(fourCC))
				{
					throw new Exception($"Magic number {fourCC} doesn't match");
				}
				return D3DHandler.TryGetShaderText(data, dataOffset, out disassemblyText);
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
