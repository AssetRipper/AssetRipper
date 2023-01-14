using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Enums;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AssetRipper.Export.UnityProjects.Textures
{
	internal static partial class CrunchHandler
	{
		public static byte[] DecompressCrunch(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, ReadOnlySpan<byte> data)
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
			return format is TextureFormat.ETC_RGB4Crunched or TextureFormat.ETC2_RGBA8Crunched;
		}

		[SupportedOSPlatform("windows")]
		[LibraryImport("crunch")]
		[UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static partial bool DecompressCRN(ReadOnlySpan<byte> pSrcFileData, int srcFileSize, out IntPtr uncompressedData, out int uncompressedSize);

		[SupportedOSPlatform("windows")]
		[LibraryImport("crunchunity")]
		[UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static partial bool DecompressUnityCRN(ReadOnlySpan<byte> pSrcFileData, int srcFileSize, out IntPtr uncompressedData, out int uncompressedSize);

		[SupportedOSPlatform("windows")]
		private static byte[] DecompressCrunchWithUtinyDecoder(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, ReadOnlySpan<byte> data)
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

		private static byte[] DecompressCrunchWithStudioDecoder(TextureFormat textureFormat, int width, int height, UnityVersion unityVersion, ReadOnlySpan<byte> data)
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

		private static bool DecompressNormalCrunchWithStudioDecoder(ReadOnlySpan<byte> data, out byte[] uncompressedBytes)
		{
			if (data.Length == 0)
			{
				throw new ArgumentException(null, nameof(data));
			}

			Logger.Info("About to unpack normal crunch...");
			uncompressedBytes = Texture2DDecoder.TextureDecoder.UnpackCrunch(data);
			return uncompressedBytes != null;
		}

		private static bool DecompressUnityCrunchWithStudioDecoder(ReadOnlySpan<byte> data, out byte[] uncompressedBytes)
		{
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
