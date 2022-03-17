using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Texture2D
{
	public interface IGLTextureSettings : IUnityAssetBase
	{
		int Aniso { get; set; }
		FilterMode FilterMode { get; set; }
		float MipBias { get; set; }
		/// <summary>
		/// Does the asset have <see cref="WrapMode"/>?<br/>
		/// On 2017 and later, this has been replaced by <see cref="WrapU"/>, <see cref="WrapV"/>, and <see cref="WrapW"/>.
		/// </summary>
		bool HasWrapMode { get; }
		/// <summary>
		/// Less than 2017
		/// </summary>
		TextureWrapMode WrapMode { get; set; }
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		TextureWrapMode WrapU { get; set; }
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		TextureWrapMode WrapV { get; set; }
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		TextureWrapMode WrapW { get; set; }
	}

	public static class GLTextureSettingsExtensions
	{
		public static void SetToDefault(this IGLTextureSettings settings)
		{
			settings.FilterMode = (FilterMode)(-1);
			settings.Aniso = -1;
			settings.MipBias = -100;
			settings.WrapMode = (TextureWrapMode)(-1);
			settings.WrapU = (TextureWrapMode)(-1);
			settings.WrapV = (TextureWrapMode)(-1);
			settings.WrapW = (TextureWrapMode)(-1);
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
