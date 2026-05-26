using AssetRipper.SourceGenerated.Subclasses.SerializedShaderFloatValue;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedShaderFloatValueExtensions
{
	extension(ISerializedShaderFloatValue floatValue)
	{
		public bool IsZero() => floatValue.Value == 0.0f;
		public bool IsMax() => floatValue.Value == 255.0f;
		public Utf8String Name
		{
			get => floatValue.Has_Name_R_Utf8String() ? floatValue.Name_R_Utf8String : floatValue.Name_R_FastPropertyName.Name;
			set
			{
				if (floatValue.Has_Name_R_Utf8String())
				{
					floatValue.Name_R_Utf8String = value;
				}
				else
				{
					floatValue.Name_R_FastPropertyName.Name = value;
				}
			}
		}
	}
}
