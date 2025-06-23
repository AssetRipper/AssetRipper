using AssetRipper.SourceGenerated.Subclasses.SerializedShaderVectorValue;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderVectorValueExtensions
{
	public static bool IsZero(this ISerializedShaderVectorValue value)
	{
		return value.X.IsZero()
			&& value.Y.IsZero()
			&& value.Z.IsZero()
			&& value.W.IsZero();
	}
}
