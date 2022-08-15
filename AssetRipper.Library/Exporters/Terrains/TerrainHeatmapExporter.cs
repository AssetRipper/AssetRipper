using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_156;

namespace AssetRipper.Library.Exporters.Terrains
{
	public sealed class TerrainHeatmapExporter : BinaryAssetExporter
	{
		public ImageExportFormat ImageFormat { get; }
		public TerrainHeatmapExporter(LibraryConfiguration configuration)
		{
			ImageFormat = configuration.ImageExportFormat;
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is ITerrainData;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new TerrainHeatmapExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			ITerrainData terrain = (ITerrainData)asset;
			using DirectBitmap bitmap = GetBitmap(terrain);
			if (bitmap == null)
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{terrain.NameString}' to bitmap");
				return false;
			}
			if (OperatingSystem.IsWindows())
			{
				bitmap.Save(path, ImageFormat);
			}
			else
			{
				TaskManager.AddTask(bitmap.SaveAsync(path, ImageFormat));
			}
			return true;
		}

		public static DirectBitmap GetBitmap(ITerrainData terrain)
		{
			DirectBitmap bitmap = new DirectBitmap(
				Math.Max(terrain.Heightmap_C156.Width, terrain.Heightmap_C156.Resolution),
				Math.Max(terrain.Heightmap_C156.Height, terrain.Heightmap_C156.Resolution),
				GetBGRA32Data(terrain));
			bitmap.FlipY();
			return bitmap;
		}

		public static byte[] GetBGRA32Data(ITerrainData terrain)
		{
			short[] heights = terrain.Heightmap_C156.Heights;
			byte[] result = new byte[heights.Length * 4];
			for (int y = 0; y < heights.Length; y++)
			{
				Color32 color = (Color32)ConvertToColor((float)heights[y] / short.MaxValue);
				result[4 * y] = color.B;
				result[(4 * y) + 1] = color.G;
				result[(4 * y) + 2] = color.R;
				result[(4 * y) + 3] = byte.MaxValue; //small optimization
			}
			return result;
		}

		private static ColorFloat ConvertToColor(float value)
		{
			if (value <= 0f)
			{
				return zero;
			}
			else if (value < q1point)
			{
				return Average(zero, 0f, q1, q1point, value);
			}
			else if (value < q2point)
			{
				return Average(q1, q1point, q2, q2point, value);
			}
			else if (value < q3point)
			{
				return Average(q2, q2point, q3, q3point, value);
			}
			else if (value < 1f)
			{
				return Average(q3, q3point, one, 1f, value);
			}
			else
			{
				return one;
			}
		}

		private static float Normalize(float min, float max, float value)
		{
			if (value <= min)
			{
				return 0f;
			}
			else if (value >= max)
			{
				return 1f;
			}
			else
			{
				return (value - min) / (max - min);
			}
		}

		private static ColorFloat Average(ColorFloat minColor, float min, ColorFloat maxColor, float max, float value)
		{
			float normalized = Normalize(min, max, value);
			return ((1 - normalized) * minColor) + (normalized * maxColor);
		}

		private static readonly ColorFloat zero = new(0, 0, 0.4f, 1);
		private static readonly ColorFloat q1 = new(0, 0, 1, 1);
		private static readonly ColorFloat q2 = new(0, 1, 0, 1);
		private static readonly ColorFloat q3 = new(1, 0, 0, 1);
		private static readonly ColorFloat one = ColorFloat.White;

		private const float q1point = 0.15f;
		private const float q2point = 0.3f;
		private const float q3point = 0.6f;
	}
}
