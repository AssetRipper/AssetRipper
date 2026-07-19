using AssetRipper.SourceGenerated.Subclasses.SerializedShaderVectorValue;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderVectorValueExtensions
{
	extension(ISerializedShaderVectorValue value)
	{
		public bool IsZero
		{
			get
			{
				return value.X.IsZero
					&& value.Y.IsZero
					&& value.Z.IsZero
					&& value.W.IsZero;
			}
		}
	}
}
