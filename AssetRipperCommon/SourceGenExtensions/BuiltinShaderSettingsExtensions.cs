using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.SourceGenerated.Subclasses.BuiltinShaderSettings;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class BuiltinShaderSettingsExtensions
	{
		public static BuiltinShaderMode GetMode(this IBuiltinShaderSettings settings)
		{
			return (BuiltinShaderMode)settings.Mode;
		}
	}
}
