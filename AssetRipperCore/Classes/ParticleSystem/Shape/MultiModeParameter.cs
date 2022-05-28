using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem.Shape
{
	public sealed class MultiModeParameter : IAssetReadable, IYamlExportable
	{
		public MultiModeParameter() { }
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
		public static bool IsConditionalValue(UnityVersion version) => version.IsGreaterEqual(2018, 3);

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ValueName, Value);
			node.Add(ModeName, (int)Mode);
			node.Add(SpreadName, Spread);
			node.Add(SpeedName, Speed.ExportYaml(container));
			return node;
		}

		public float Value { get; set; }
		public ParticleSystemShapeMultiModeValue Mode { get; set; }
		public float Spread { get; set; }

		public const string ValueName = "value";
		public const string ModeName = "mode";
		public const string SpreadName = "spread";
		public const string SpeedName = "speed";

		public MinMaxCurve Speed = new();
	}
}
