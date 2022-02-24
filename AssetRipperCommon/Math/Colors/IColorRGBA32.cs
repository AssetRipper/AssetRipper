using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Colors
{
	public interface IColorRGBA32 : IAsset
	{
		uint RGBA { get; set; }
	}

	public static class ColorRGBA32Extensions
	{
		public static byte R(this IColorRGBA32 color) => (byte)(color.RGBA >> 0 & 0xFF);
		public static byte G(this IColorRGBA32 color) => (byte)(color.RGBA >> 8 & 0xFF);
		public static byte B(this IColorRGBA32 color) => (byte)(color.RGBA >> 16 & 0xFF);
		public static byte A(this IColorRGBA32 color) => (byte)(color.RGBA >> 24 & 0xFF);
	}
}
