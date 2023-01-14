using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SerializedPropertyExtensions
	{
		public static SerializedPropertyType GetType_(this ISerializedProperty property)
		{
			return (SerializedPropertyType)property.Type;
		}

		public static SerializedPropertyFlag GetFlags(this ISerializedProperty property)
		{
			return (SerializedPropertyFlag)property.Flags;
		}
	}
}
