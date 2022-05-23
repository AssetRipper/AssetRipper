using AssetRipper.SourceGenerated.Subclasses.GLTextureSettings;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class GLTextureSettingsExtensions
	{
		public static void SetToDefault(this IGLTextureSettings settings)
		{
			settings.FilterMode = (int)(Classes.Texture2D.FilterMode)(-1);
			settings.Aniso = -1;
			settings.MipBias = -100;
			settings.WrapMode = (int)(Classes.Texture2D.TextureWrapMode)(-1);
			settings.WrapU = (int)(Classes.Texture2D.TextureWrapMode)(-1);
			settings.WrapV = (int)(Classes.Texture2D.TextureWrapMode)(-1);
			settings.WrapW = (int)(Classes.Texture2D.TextureWrapMode)(-1);
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
}
