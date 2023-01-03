using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.SourceGenerated.Subclasses.ShaderBindChannel;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ShaderBindChannelExtensions
	{
		public static VertexComponent GetTarget(this IShaderBindChannel channel)
		{
			return (VertexComponent)channel.Target;
		}
	}
}
