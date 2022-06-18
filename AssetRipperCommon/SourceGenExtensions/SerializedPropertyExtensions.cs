using AssetRipper.Core.Classes.Shader.SerializedShader.Enum;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;

namespace AssetRipper.Core.SourceGenExtensions
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
