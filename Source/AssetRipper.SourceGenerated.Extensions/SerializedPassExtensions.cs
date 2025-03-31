using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedPass;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SerializedPassExtensions
	{
		public static SerializedPassType GetType_(this ISerializedPass pass)
		{
			return (SerializedPassType)pass.Type;
		}
	}
}
