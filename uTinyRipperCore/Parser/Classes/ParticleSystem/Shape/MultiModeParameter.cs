using uTinyRipper.Converters;
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
		public static bool IsConditionalValue(Version version) => version.IsGreaterEqual(2018, 3);

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
			node.Add(ValueName, Value);
			node.Add(ModeName, (int)Mode);
			node.Add(SpreadName, Spread);
			node.Add(SpeedName, Speed.ExportYAML(container));
			return node;
		}

		public float Value { get; set; }
		public ParticleSystemShapeMultiModeValue Mode { get; set; }
		public float Spread { get; set; }

		public const string ValueName = "value";
		public const string ModeName = "mode";
		public const string SpreadName = "spread";
		public const string SpeedName = "speed";

		public MinMaxCurve Speed;
	}
}
