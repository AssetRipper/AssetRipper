using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct ColorRGBA32 : IAssetReadable, IYAMLExportable
	{
		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			// it's min version
			return 2;
		}

		public void Read(AssetStream stream)
		{
			RGBA = stream.ReadUInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("rgba", RGBA);
			return node;
		}

		public uint RGBA { get; private set; }
	}
}
