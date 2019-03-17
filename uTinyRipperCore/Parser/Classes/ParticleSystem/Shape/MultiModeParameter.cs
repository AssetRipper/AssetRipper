using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
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

		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsConditionalValue(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		public void Read(AssetReader reader)
		{
			Read(reader, true);
		}

		public void Read(AssetReader reader, bool readValue)
		{
			if (!IsConditionalValue(reader.Version) || readValue)
			{
				Value = reader.ReadSingle();
			}
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
