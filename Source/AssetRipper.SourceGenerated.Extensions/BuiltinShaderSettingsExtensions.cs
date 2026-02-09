using AssetRipper.SourceGenerated.Subclasses.BuiltinShaderSettings;
using BuiltinShaderMode = AssetRipper.SourceGenerated.Enums.BuiltinShaderMode_1;

namespace AssetRipper.SourceGenerated.Extensions;

public static class BuiltinShaderSettingsExtensions
{
	public static BuiltinShaderMode GetMode(this IBuiltinShaderSettings settings)
	{
		return (BuiltinShaderMode)settings.Mode;
	}
}
