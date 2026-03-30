using AssetRipper.SourceGenerated.Subclasses.SerializedShaderFloatValue;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderFloatValueExtensions
{
	public static bool IsZero(this ISerializedShaderFloatValue value) => value.Value == 0.0f;
	public static bool IsMax(this ISerializedShaderFloatValue value) => value.Value == 255.0f;
}
