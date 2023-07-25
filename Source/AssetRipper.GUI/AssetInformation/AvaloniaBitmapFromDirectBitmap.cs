using Avalonia.Media.Imaging;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.AssetInformation
{
	public static class AvaloniaBitmapFromDirectBitmap
	{
		/// <summary>
		/// Converts a DirectBitmap into an Avalonia Bitmap. Disposes the DirectBitmap.
		/// </summary>
		public static Bitmap Make(DirectBitmap directBitmap)
		{
			MemoryStream resultStream = new();
			directBitmap.SaveAsPng(resultStream);
			resultStream.Seek(0, SeekOrigin.Begin);
			return new Bitmap(resultStream);
		}
	}
}
