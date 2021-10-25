using AssetRipper.Core;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.TextureDecoders.Rgb;
using AssetRipper.Library.Utils;
using System.IO;

namespace AssetRipper.Library.Exporters.Terrains
{
	public class TerrainHeatmapExporter : BinaryAssetExporter
	{
		TerrainExportMode ExportMode { get; }
		ImageExportFormat ImageFormat { get; }
		public TerrainHeatmapExporter(LibraryConfiguration configuration)
		{
			ExportMode = configuration.TerrainExportMode;
			ImageFormat = configuration.ImageExportFormat;
		}

		public override bool IsHandle(UnityObjectBase asset)
		{
			return ExportMode == TerrainExportMode.Heatmap;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset, ImageFormat.GetFileExtension());
		}

		public override bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			TerrainData terrain = (TerrainData)asset;
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				if (!ExportImage(terrain, fileStream))
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{terrain.Name}' to bitmap");
					return false;
				}
			}
			return true;
		}

		private bool ExportImage(TerrainData terrain, Stream exportStream)
		{
			using DirectBitmap bitmap = GetBitmap(terrain);
			if (bitmap == null)
			{
				return false;
			}
			return bitmap.Save(exportStream, ImageFormat);
		}

		public static DirectBitmap GetBitmap(TerrainData terrain)
		{
			int width = terrain.Heightmap.Width;
			int height = terrain.Heightmap.Height;
			byte[] data = GetRGBA32Data(terrain);
			DirectBitmap bitmap = new DirectBitmap(width, height);
			RgbConverter.RGBA32ToBGRA32(data, width, height, bitmap.Bits);
			bitmap.FlipY();
			return bitmap;
		}

		public static byte[] GetRGBA32Data(TerrainData terrain)
		{
			short[] heights = terrain.Heightmap.Heights;
			byte[] result = new byte[heights.Length * 4];
			for (int y = 0; y < heights.Length; y++)
			{
				ColorRGBA32 color = (ColorRGBA32)ConvertToColor((float)heights[y] / short.MaxValue);
				result[4 * y] = color.R;
				result[4 * y + 1] = color.G;
				result[4 * y + 2] = color.B;
				result[4 * y + 3] = byte.MaxValue; //small optimization
			}
			return result;
		}

		private static ColorRGBAf ConvertToColor(float value)
		{
			if (value <= 0f)
				return zero;
			else if (value < q1point)
				return Average(zero, 0f, q1, q1point, value);
			else if (value < q2point)
				return Average(q1, q1point, q2, q2point, value);
			else if (value < q3point)
				return Average(q2, q2point, q3, q3point, value);
			else if (value < 1f)
				return Average(q3, q3point, one, 1f, value);
			else
				return one;
		}

		private static float Normalize(float min, float max, float value)
		{
			if (value <= min)
				return 0f;
			else if (value >= max)
				return 1f;
			else
				return (value - min) / (max - min);
		}

		private static ColorRGBAf Average(ColorRGBAf minColor, float min, ColorRGBAf maxColor, float max, float value)
		{
			float normalized = Normalize(min, max, value);
			return (1 - normalized) * minColor + normalized * maxColor;
		}

		private static ColorRGBAf zero = new ColorRGBAf(0, 0, .4f, 1);
		private static ColorRGBAf q1 = new ColorRGBAf(0, 0, 1, 1);
		private static ColorRGBAf q2 = new ColorRGBAf(0, 1, 0, 1);
		private static ColorRGBAf q3 = new ColorRGBAf(1, 0, 0, 1);
		private static ColorRGBAf one = ColorRGBAf.White;

		private const float q1point = .15f;
		private const float q2point = .3f;
		private const float q3point = .6f;
	}
}