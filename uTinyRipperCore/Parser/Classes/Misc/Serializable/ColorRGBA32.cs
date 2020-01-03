using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public struct ColorRGBA32 : IAsset
	{
		public ColorRGBA32(byte r, byte g, byte b, byte a)
		{
			RGBA = unchecked((uint)(r | (g << 8) | (b << 16) | (a << 24)));
		}

		public static explicit operator ColorRGBA32(ColorRGBAf color)
		{
			byte r = (byte)(color.R * 255.0f);
			byte g = (byte)(color.G * 255.0f);
			byte b = (byte)(color.B * 255.0f);
			byte a = (byte)(color.A * 255.0f);
			return new ColorRGBA32(r, g, b, a);
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
			ColorRGBA32Layout layout = container.ExportLayout.Serialized.ColorRGBA32;
			node.AddSerializedVersion(layout.Version);
			node.Add(layout.RgbaName, RGBA);
			return node;
		}

		public static ColorRGBA32 White => new ColorRGBA32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public byte R => (byte)((RGBA >> 0) & 0xFF);
		public byte G => (byte)((RGBA >> 8) & 0xFF);
		public byte B => (byte)((RGBA >> 16) & 0xFF);
		public byte A => (byte)((RGBA >> 24) & 0xFF);

		public uint RGBA { get; set; }
	}
}
