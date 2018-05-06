using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct Rectf : IAssetReadable, IYAMLExportable
	{
		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public void Read(AssetStream stream)
		{
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
			Width = stream.ReadSingle();
			Height = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("width", Width);
			node.Add("height", Height);
			return node;
		}

		public float X { get; private set; }
		public float Y { get; private set; }
		public float Width { get; private set; }
		public float Height { get; private set; }
	}
}
