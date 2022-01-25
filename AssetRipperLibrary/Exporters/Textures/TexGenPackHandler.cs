using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Library.Exporters.Textures.Enums;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AssetRipper.Library.Exporters.Textures
{
	internal static class TexGenPackHandler
	{
		[SupportedOSPlatform("windows")]
		[DllImport("texgenpack", CallingConvention = CallingConvention.Cdecl)]
		private static extern void texgenpackdecode(int texturetype, byte[] texturedata, int width, int height, IntPtr bmp, bool fixAlpha);

		public static bool IsAvailable() => OperatingSystem.IsWindows();

		public static bool Decode(TextureFormat textureFormat, byte[] texturedata, int width, int height, IntPtr bmp, bool fixAlpha)
		{
			if (!OperatingSystem.IsWindows())
				return false;
			texgenpackdecode((int)ToTexgenpackTexturetype(textureFormat), texturedata, width, height, bmp, fixAlpha);
			return true;
		}

		private static TexgenpackTexturetype ToTexgenpackTexturetype(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.BC4:
					return TexgenpackTexturetype.RGTC1;

				case TextureFormat.BC5:
					return TexgenpackTexturetype.RGTC2;

				case TextureFormat.BC6H:
					return TexgenpackTexturetype.BPTC_FLOAT;

				case TextureFormat.BC7:
					return TexgenpackTexturetype.BPTC;

				default:
					throw new NotSupportedException(format.ToString());
			}
		}
	}
}
