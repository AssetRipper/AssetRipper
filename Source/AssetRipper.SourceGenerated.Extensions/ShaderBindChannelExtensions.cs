using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using AssetRipper.SourceGenerated.Subclasses.ShaderBindChannel;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ShaderBindChannelExtensions
{
	public static VertexComponent GetTarget(this IShaderBindChannel channel)
	{
		return (VertexComponent)channel.Target;
	}
}
