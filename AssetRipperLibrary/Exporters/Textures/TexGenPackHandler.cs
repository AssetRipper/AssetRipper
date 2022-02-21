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
			return format switch
			{
				TextureFormat.BC4 => TexgenpackTexturetype.RGTC1,
				TextureFormat.BC5 => TexgenpackTexturetype.RGTC2,
				TextureFormat.BC6H => TexgenpackTexturetype.BPTC_FLOAT,
				TextureFormat.BC7 => TexgenpackTexturetype.BPTC,
				_ => throw new NotSupportedException(format.ToString()),
			};
		}
	}
}
