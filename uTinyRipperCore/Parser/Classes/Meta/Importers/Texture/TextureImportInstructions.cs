using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TextureImporters
{
	public struct TextureImportInstructions : IAsset
	{
		public TextureImportInstructions(AssetLayout layout) :
			this()
		{
#warning TODO: default values
		}

		/// <summary>
		/// Less than 5.5.0 and greater
		/// </summary>
		public static bool HasRecommendedFormat(Version version) => version.IsLess(5, 5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasDesiredFormat(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasAndroidETC2Fallback(Version version) => version.IsGreaterEqual(2017, 3);

		public void Read(AssetReader reader)
		{
			CompressedFormat = reader.ReadInt32();
			UncompressedFormat = reader.ReadInt32();
			if (HasRecommendedFormat(reader.Version))
			{
				RecommendedFormat = reader.ReadInt32();
			}
			if (HasDesiredFormat(reader.Version))
			{
				DesiredFormat = reader.ReadInt32();
			}
			UsageMode = reader.ReadInt32();
			ColorSpace = reader.ReadInt32();
			if (HasAndroidETC2Fallback(reader.Version))
			{
				AndroidETC2FallbackFormat = reader.ReadInt32();
			}
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			CompressionQuality = reader.ReadInt32();
			if (HasAndroidETC2Fallback(reader.Version))
			{
				AndroidETC2FallbackDownscale = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(CompressedFormat);
			writer.Write(UncompressedFormat);
			if (HasRecommendedFormat(writer.Version))
			{
				writer.Write(RecommendedFormat);
			}
			if (HasDesiredFormat(writer.Version))
			{
				writer.Write(DesiredFormat);
			}
			writer.Write(UsageMode);
			writer.Write(ColorSpace);
			if (HasAndroidETC2Fallback(writer.Version))
			{
				writer.Write(AndroidETC2FallbackFormat);
			}
			writer.Write(Width);
			writer.Write(Height);
			writer.Write(CompressionQuality);
			if (HasAndroidETC2Fallback(writer.Version))
			{
				writer.Write(AndroidETC2FallbackDownscale);
				writer.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(CompressedFormatName, CompressedFormat);
			node.Add(UncompressedFormatName, UncompressedFormat);
			if (HasRecommendedFormat(container.ExportVersion))
			{
				node.Add(RecommendedFormatName, RecommendedFormat);
			}
			if (HasDesiredFormat(container.ExportVersion))
			{
				node.Add(DesiredFormatName, DesiredFormat);
			}
			node.Add(UsageModeName, UsageMode);
			node.Add(ColorSpaceName, ColorSpace);
			if (HasAndroidETC2Fallback(container.ExportVersion))
			{
				node.Add(AndroidETC2FallbackFormatName, AndroidETC2FallbackFormat);
			}
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(CompressionQualityName, CompressionQuality);
			if (HasAndroidETC2Fallback(container.ExportVersion))
			{
				node.Add(AndroidETC2FallbackDownscaleName, AndroidETC2FallbackDownscale);
			}
			return node;
		}

		public int CompressedFormat { get; set; }
		public int UncompressedFormat { get; set; }
		public int RecommendedFormat { get; set; }
		public int DesiredFormat { get; set; }
		public int UsageMode { get; set; }
		public int ColorSpace { get; set; }
		public int AndroidETC2FallbackFormat { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int CompressionQuality { get; set; }
		public bool AndroidETC2FallbackDownscale { get; set; }

		public const string CompressedFormatName = "compressedFormat";
		public const string UncompressedFormatName = "uncompressedFormat";
		public const string RecommendedFormatName = "recommendedFormat";
		public const string DesiredFormatName = "desiredFormat";
		public const string UsageModeName = "usageMode";
		public const string ColorSpaceName = "colorSpace";
		public const string AndroidETC2FallbackFormatName = "androidETC2FallbackFormat";
		public const string WidthName = "width";
		public const string HeightName = "height";
		public const string CompressionQualityName = "compressionQuality";
		public const string AndroidETC2FallbackDownscaleName = "androidETC2FallbackDownscale";
	}
}
