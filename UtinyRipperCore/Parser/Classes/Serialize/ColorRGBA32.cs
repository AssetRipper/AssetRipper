using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public struct ColorRGBA32 : IScriptStructure
	{
		public ColorRGBA32(ColorRGBAf rgba)
		{
			byte r = (byte)(rgba.R * 255.0f);
			byte g = (byte)(rgba.G * 255.0f);
			byte b = (byte)(rgba.B * 255.0f);
			byte a = (byte)(rgba.A * 255.0f);
			RGBA = unchecked((uint)(r | (g << 8) | (b << 16) | (a << 24)));
		}

		public ColorRGBA32(ColorRGBA32 copy)
		{
			RGBA = copy.RGBA;
		}

		public ColorRGBA32(byte r, byte g, byte b, byte a)
		{
			RGBA = unchecked((uint)(r | (g << 8) | (b << 16) | (a << 24)));
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			// it's min version
			return 2;
		}

		public IScriptStructure CreateCopy()
		{
			return new ColorRGBA32(this);
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
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("rgba", RGBA);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.Color32Name;

		public uint RGBA { get; private set; }
	}
}
