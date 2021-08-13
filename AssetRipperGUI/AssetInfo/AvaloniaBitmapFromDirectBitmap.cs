using AssetRipper.Library.Utils;
using Avalonia.Media.Imaging;
using System.Drawing.Imaging;
using System.IO;

namespace AssetRipper.GUI.AssetInfo
{
	public static class AvaloniaBitmapFromDirectBitmap
	{
		public static Bitmap Make(DirectBitmap directBitmap)
		{
			MemoryStream resultStream = new();
			directBitmap.Save(resultStream, ImageFormat.Png);
			
			directBitmap.Dispose();

			resultStream.Seek(0, SeekOrigin.Begin);

			return new Bitmap(resultStream);
		}
	}
}