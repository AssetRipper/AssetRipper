using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.SourceGenerated.Subclasses.SerializedPass;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SerializedPassExtensions
	{
		public static SerializedPassType GetType_(this ISerializedPass pass)
		{
			return (SerializedPassType)pass.Type;
		}
	}
}
