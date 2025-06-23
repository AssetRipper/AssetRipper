using AssetRipper.Export.UnityProjects.Textures.Enums;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.Export.UnityProjects.Textures.Extensions;

public static class Texture2DExtensions
{
	/// <summary>
	/// Get the applicable KTX base format for a texture
	/// </summary>
	/// <param name="_this">The texture</param>
	/// <param name="safe">If true, this method will return RG rather than throwing an exception.</param>
	/// <returns>The format</returns>
	/// <exception cref="NotSupportedException">if not safe and not an pvrtc texture</exception>
	public static KTXBaseInternalFormat GetKTXBaseInternalFormat(this SourceGenerated.Classes.ClassID_28.ITexture2D _this, bool safe)
	{
		switch (_this.Format_C28E)
		{
			case TextureFormat.RHalf:
			case TextureFormat.RFloat:
			case TextureFormat.BC4:
			case TextureFormat.EAC_R:
			case TextureFormat.EAC_R_SIGNED:
				return KTXBaseInternalFormat.RED;

			case TextureFormat.RGHalf:
			case TextureFormat.RGFloat:
			case TextureFormat.BC5:
			case TextureFormat.EAC_RG:
			case TextureFormat.EAC_RG_SIGNED:
				return KTXBaseInternalFormat.RG;

			case TextureFormat.BC6H:
			case TextureFormat.PVRTC_RGB2:
			case TextureFormat.PVRTC_RGB4:
			case TextureFormat.ETC_RGB4Crunched:
			case TextureFormat.ETC_RGB4_3DS:
			case TextureFormat.ETC_RGB4:
			case TextureFormat.ATC_RGB4:
			case TextureFormat.ETC2_RGB:
				return KTXBaseInternalFormat.RGB;

			case TextureFormat.RGBAHalf:
			case TextureFormat.RGBAFloat:
			case TextureFormat.BC7:
			case TextureFormat.PVRTC_RGBA2:
			case TextureFormat.PVRTC_RGBA4:
			case TextureFormat.ATC_RGBA8:
			case TextureFormat.ETC2_RGBA8Crunched:
			case TextureFormat.ETC_RGBA8_3DS:
			case TextureFormat.ETC2_RGBA8:
			case TextureFormat.ETC2_RGBA1:
				return KTXBaseInternalFormat.RGBA;

			default:
				{
					if (safe)
					{
						return KTXBaseInternalFormat.RG;
					}
					else
					{
						throw new NotSupportedException(_this.Format_C28E.ToString());
					}
				}
		}
	}
}
