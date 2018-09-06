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

		public void Read(AssetReader reader)
		{
			Value = reader.ReadSingle();
			Mode = (ParticleSystemShapeMultiModeValue)reader.ReadInt32();
			Spread = reader.ReadSingle();
			Speed.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("value", Value);
			node.Add("mode", (int)Mode);
			node.Add("spread", Spread);
			node.Add("speed", Speed.ExportYAML(container));
			return node;
		}

		public float Value { get; private set; }
		public ParticleSystemShapeMultiModeValue Mode { get; private set; }
		public float Spread { get; private set; }

		public MinMaxCurve Speed;
	}
}
