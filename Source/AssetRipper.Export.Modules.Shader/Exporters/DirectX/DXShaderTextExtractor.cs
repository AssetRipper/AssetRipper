using AssetRipper.Export.Modules.Shaders.Handlers;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader;

namespace AssetRipper.Export.Modules.Shaders.Exporters.DirectX;

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
