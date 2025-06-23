using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.GLTextureSettings;
using FilterMode = AssetRipper.SourceGenerated.Enums.FilterMode_0;

namespace AssetRipper.SourceGenerated.Extensions;

public static class GLTextureSettingsExtensions
{
	public static void SetToDefault(this IGLTextureSettings settings)
	{
		settings.FilterMode = (int)(FilterMode)(-1);
		settings.Aniso = -1;
		settings.MipBias = -100;
		settings.WrapMode = (int)(TextureWrapMode)(-1);
		settings.WrapU = (int)(TextureWrapMode)(-1);
		settings.WrapV = (int)(TextureWrapMode)(-1);
		settings.WrapW = (int)(TextureWrapMode)(-1);
	}

	public static void CopyValues(this IGLTextureSettings destination, IGLTextureSettings source)
	{
		destination.Aniso = source.Aniso;
		destination.FilterMode = source.FilterMode;
		destination.MipBias = source.MipBias;
		destination.WrapMode = source.WrapMode;
		destination.WrapU = source.WrapU;
		destination.WrapV = source.WrapV;
		destination.WrapW = source.WrapW;
	}
}
