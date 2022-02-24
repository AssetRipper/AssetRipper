using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Math.Colors
{
	public sealed class ColorRGBA32 : IAsset, IColorRGBA32
	{
		public ColorRGBA32() { }

		public ColorRGBA32(byte r, byte g, byte b, byte a)
		{
			RGBA = unchecked((uint)(r | g << 8 | b << 16 | a << 24));
		}

		public static explicit operator ColorRGBA32(ColorRGBAf color)
		{
			byte r = ConvertFloatToByte(color.R);
			byte g = ConvertFloatToByte(color.G);
			byte b = ConvertFloatToByte(color.B);
			byte a = ConvertFloatToByte(color.A);
			return new ColorRGBA32(r, g, b, a);
		}

		private static byte ConvertFloatToByte(float value)
		{
			if (float.IsNaN(value)) return byte.MinValue;
			float scaledValue = value * 255.0f;
			if (scaledValue <= 0f) return byte.MinValue;
			if (scaledValue >= 255f) return byte.MaxValue;
			return (byte)scaledValue;
		}

		public void Read(AssetReader reader)
		{
			RGBA = reader.ReadUInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(RGBA);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion());
			node.Add(RgbaName, RGBA);
			return node;
		}

		/// <summary>
		/// NOTE: min version is 2
		/// </summary>
		public static int ToSerializedVersion() => 2;

		public static ColorRGBA32 Black => new ColorRGBA32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);
		public static ColorRGBA32 White => new ColorRGBA32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public uint RGBA { get; set; }

		public const string RgbaName = "rgba";
	}
}
