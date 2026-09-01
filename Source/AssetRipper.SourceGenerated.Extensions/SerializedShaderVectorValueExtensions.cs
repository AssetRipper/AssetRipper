using AssetRipper.SourceGenerated.Subclasses.SerializedShaderVectorValue;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderVectorValueExtensions
{
	extension(ISerializedShaderVectorValue vectorValue)
	{
		public bool IsZero
		{
			get
			{
				return vectorValue.X.IsZero
					&& vectorValue.Y.IsZero
					&& vectorValue.Z.IsZero
					&& vectorValue.W.IsZero;
			}
		}
		public bool HasName => vectorValue.Name.String is not "" and not "<noninit>";
		public bool IsZeroAndNameless => vectorValue.IsZero && !vectorValue.HasName;
		public Utf8String Name
		{
			get => vectorValue.Has_Name_R_Utf8String() ? vectorValue.Name_R_Utf8String : vectorValue.Name_R_FastPropertyName.Name;
			set
			{
				if (vectorValue.Has_Name_R_Utf8String())
				{
					vectorValue.Name_R_Utf8String = value;
				}
				else
				{
					vectorValue.Name_R_FastPropertyName.Name = value;
				}
			}
		}
		public string GetNameOrVectorString()
		{
			if (vectorValue.HasName)
			{
				return $"[{vectorValue.Name}]";
			}
			return $"({vectorValue.X.Value.ToStringInvariant()}, {vectorValue.Y.Value.ToStringInvariant()}, {vectorValue.Z.Value.ToStringInvariant()}, {vectorValue.W.Value.ToStringInvariant()})";
		}
	}
}
