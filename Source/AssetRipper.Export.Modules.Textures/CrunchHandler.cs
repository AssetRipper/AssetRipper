using AssetRipper.Conversions.Crunch;
using AssetRipper.Conversions.UnityCrunch;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.Export.UnityProjects.Textures;

internal static class CrunchHandler
{
	public static bool DecompressCrunch(TextureFormat textureFormat, UnityVersion unityVersion, ReadOnlySpan<byte> data, [NotNullWhen(true)] out byte[]? uncompressedBytes)
	{
		return IsUseUnityCrunch(unityVersion, textureFormat)
			? UnityCrunch.TryDecompress(data, out uncompressedBytes)
			: Crunch.TryDecompress(data, out uncompressedBytes);
	}

	private static bool IsUseUnityCrunch(UnityVersion version, TextureFormat format)
	{
		if (version.GreaterThanOrEquals(2017, 3))
		{
			return true;
		}
		return format is TextureFormat.ETC_RGB4Crunched or TextureFormat.ETC2_RGBA8Crunched;
	}
}
