using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Logging;
using AssetRipper.SourceGenerated.Enums;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AssetRipper.Library.Exporters.Textures
{
	internal static class CrunchHandler
	{
		public static byte[] DecompressCrunch(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, byte[] data)
		{
			if (OperatingSystem.IsWindows())
			{
				return DecompressCrunchWithUtinyDecoder(textureFormat, width, height, unityVersion, data);
			}
			else
			{
				return DecompressCrunchWithStudioDecoder(textureFormat, width, height, unityVersion, data);
			}
		}

		private static bool IsUseUnityCrunch(UnityVersion version, TextureFormat format)
		{
			if (version.IsGreaterEqual(2017, 3))
			{
				return true;
			}
			return format == TextureFormat.ETC_RGB4Crunched || format == TextureFormat.ETC2_RGBA8Crunched;
		}

		[SupportedOSPlatform("windows")]
		[DllImport("crunch", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DecompressCRN(byte[] pSrcFileData, int srcFileSize, out IntPtr uncompressedData, out int uncompressedSize);

		[SupportedOSPlatform("windows")]
		[DllImport("crunchunity", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DecompressUnityCRN(byte[] pSrc_file_data, int src_file_size, out IntPtr uncompressedData, out int uncompressedSize);

		[SupportedOSPlatform("windows")]
		private static byte[] DecompressCrunchWithUtinyDecoder(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, byte[] data)
		{
			IntPtr uncompressedData = default;
			try
			{
				bool result = IsUseUnityCrunch(unityVersion, textureFormat) ?
					DecompressUnityCRN(data, data.Length, out uncompressedData, out int uncompressedSize) :
					DecompressCRN(data, data.Length, out uncompressedData, out uncompressedSize);
				if (result)
				{
					byte[] uncompressedBytes = new byte[uncompressedSize];
					Marshal.Copy(uncompressedData, uncompressedBytes, 0, uncompressedSize);
					return uncompressedBytes;
				}
				else
				{
					throw new Exception("Unable to decompress crunched texture");
				}
			}
			finally
			{
				Marshal.FreeHGlobal(uncompressedData);
			}
		}

		private static byte[] DecompressCrunchWithStudioDecoder(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, byte[] data)
		{
			bool result = IsUseUnityCrunch(unityVersion, textureFormat) ?
					DecompressUnityCrunchWithStudioDecoder(data, out byte[] uncompressedBytes) :
					DecompressNormalCrunchWithStudioDecoder(data, out uncompressedBytes);
			if (result)
			{
				return uncompressedBytes;
			}
			else
			{
				throw new Exception("Unable to decompress crunched texture");
			}
		}

		private static bool DecompressNormalCrunchWithStudioDecoder(byte[] data, out byte[] uncompressedBytes)
		{
			if (data is null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (data.Length == 0)
			{
				throw new ArgumentException(null, nameof(data));
			}

			Logger.Info("About to unpack normal crunch...");
			uncompressedBytes = Texture2DDecoder.TextureDecoder.UnpackCrunch(data);
			return uncompressedBytes != null;
		}

		private static bool DecompressUnityCrunchWithStudioDecoder(byte[] data, out byte[] uncompressedBytes)
		{
			if (data is null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (data.Length == 0)
			{
				throw new ArgumentException(null, nameof(data));
			}

			Logger.Info("About to unpack unity crunch...");
			uncompressedBytes = Texture2DDecoder.TextureDecoder.UnpackUnityCrunch(data);
			return uncompressedBytes != null;
		}
	}
}
