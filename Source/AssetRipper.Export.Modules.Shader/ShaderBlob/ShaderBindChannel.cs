using AssetRipper.SourceGenerated.Extensions.Enums.Shader;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob;

/// <summary>
/// SerializedBindChannel
/// </summary>
/// <param name="Source">ShaderChannel enum</param>
/// <param name="Target"></param>
public sealed record ShaderBindChannel(uint Source, VertexComponent Target)
{
}
