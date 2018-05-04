using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct MultiModeParameter : IAssetReadable, IYAMLExportable
	{
		public MultiModeParameter(float value)
		{
			Value = value;
			Mode = ParticleSystemShapeMultiModeValue.Random;
			Spread = 0.0f;
			Speed = new MinMaxCurve(1.0f);
		}

		public void Read(AssetStream stream)
		{
			Value = stream.ReadSingle();
			Mode = (ParticleSystemShapeMultiModeValue)stream.ReadInt32();
			Spread = stream.ReadSingle();
			Speed.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("value", Value);
			node.Add("mode", (int)Mode);
			node.Add("spread", Spread);
			node.Add("speed", Speed.ExportYAML(exporter));
			return node;
		}

		public float Value { get; private set; }
		public ParticleSystemShapeMultiModeValue Mode { get; private set; }
		public float Spread { get; private set; }

		public MinMaxCurve Speed;
	}
}
