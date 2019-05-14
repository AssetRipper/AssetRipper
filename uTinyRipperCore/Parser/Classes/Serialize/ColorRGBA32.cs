using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct ColorRGBA32 : ISerializableStructure
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

		private static int GetSerializedVersion(Version version)
		{
			// it's min version
			return 2;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new ColorRGBA32();
		}

		public void Read(AssetReader reader)
		{
			RGBA = reader.ReadUInt32();
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write(RGBA);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(RgbaName, RGBA);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public static ColorRGBA32 White => new ColorRGBA32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public uint RGBA { get; private set; }

		public const string RgbaName = "rgba";
	}
}
