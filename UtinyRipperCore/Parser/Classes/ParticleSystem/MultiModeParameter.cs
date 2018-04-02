using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct MultiModeParameter : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Value = stream.ReadSingle();
			Mode = stream.ReadInt32();
			Spread = stream.ReadSingle();
			Speed.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("value", Value);
			node.Add("mode", Mode);
			node.Add("spread", Spread);
			node.Add("speed", Speed.ExportYAML(exporter));
			return node;
		}

		public float Value { get; private set; }
		public int Mode { get; private set; }
		public float Spread { get; private set; }

		public MinMaxCurve Speed;
	}
}
