using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Enums;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AssetRipper.Export.UnityProjects.Textures
{
	internal static partial class CrunchHandler
	{
		public static bool DecompressCrunch(TextureFormat textureFormat, UnityVersion unityVersion, ReadOnlySpan<byte> data, [NotNullWhen(true)] out byte[]? uncompressedBytes)
		{
			if (OperatingSystem.IsWindows())
			{
				return DecompressCrunchWithUtinyDecoder(textureFormat, unityVersion, data, out uncompressedBytes);
			}
			else
			{
				return DecompressCrunchWithStudioDecoder(textureFormat, unityVersion, data, out uncompressedBytes);
			}
		}

		private static bool IsUseUnityCrunch(UnityVersion version, TextureFormat format)
		{
			if (version.GreaterThanOrEquals(2017, 3))
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
		private static bool DecompressCrunchWithUtinyDecoder(TextureFormat textureFormat, UnityVersion unityVersion, ReadOnlySpan<byte> data, [NotNullWhen(true)] out byte[]? uncompressedBytes)
		{
			IntPtr uncompressedData = default;
			try
			{
				bool result = IsUseUnityCrunch(unityVersion, textureFormat)
					? DecompressUnityCRN(data, data.Length, out uncompressedData, out int uncompressedSize)
					: DecompressCRN(data, data.Length, out uncompressedData, out uncompressedSize);

				if (result && uncompressedSize > 0 && uncompressedData != default)
				{
					uncompressedBytes = new byte[uncompressedSize];
					Marshal.Copy(uncompressedData, uncompressedBytes, 0, uncompressedSize);
					return true;
				}
				else
				{
					uncompressedBytes = null;
					return false;
				}
			}
			finally
			{
				if (uncompressedData != default)
				{
					Marshal.FreeHGlobal(uncompressedData);
				}
			}
		}

		private static bool DecompressCrunchWithStudioDecoder(TextureFormat textureFormat, UnityVersion unityVersion, ReadOnlySpan<byte> data, [NotNullWhen(true)] out byte[]? uncompressedBytes)
		{
			return IsUseUnityCrunch(unityVersion, textureFormat)
				? DecompressUnityCrunchWithStudioDecoder(data, out uncompressedBytes)
				: DecompressNormalCrunchWithStudioDecoder(data, out uncompressedBytes);
		}

		private static bool DecompressNormalCrunchWithStudioDecoder(ReadOnlySpan<byte> data, [NotNullWhen(true)] out byte[]? uncompressedBytes)
		{
			if (data.Length <= 0)
			{
				throw new ArgumentException(null, nameof(data));
			}

			Logger.Info("About to unpack normal crunch...");
			uncompressedBytes = Texture2DDecoder.TextureDecoder.UnpackCrunch(data);
			return uncompressedBytes is { Length: > 0 };
		}

		private static bool DecompressUnityCrunchWithStudioDecoder(ReadOnlySpan<byte> data, [NotNullWhen(true)] out byte[]? uncompressedBytes)
		{
			if (data.Length <= 0)
			{
				throw new ArgumentException(null, nameof(data));
			}

			Logger.Info("About to unpack unity crunch...");
			uncompressedBytes = Texture2DDecoder.TextureDecoder.UnpackUnityCrunch(data);
			return uncompressedBytes is { Length: > 0 };
		}
	}
}
