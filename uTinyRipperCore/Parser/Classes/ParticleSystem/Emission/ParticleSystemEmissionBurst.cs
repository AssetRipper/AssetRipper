using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public struct ParticleSystemEmissionBurst : IAssetReadable, IYAMLExportable
	{
		public ParticleSystemEmissionBurst(float time, int minValue, int maxValue)
		{
			Time = time;
			CycleCount = 1;
			RepeatInterval = 0.01f;
			CountCurve = new MinMaxCurve(minValue, maxValue);
			Probability = 1.0f;
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2017, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasCurve(Version version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasProbability(Version version) => version.IsGreaterEqual(2018, 3);

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			if (HasCurve(reader.Version))
			{
				CountCurve.Read(reader);
			}
			else
			{
				int minValue = reader.ReadInt32();
				int maxValue = reader.ReadInt32();
				CountCurve = new MinMaxCurve(minValue, maxValue);
			}
			CycleCount = reader.ReadInt32();
			RepeatInterval = reader.ReadSingle();
			if (HasProbability(reader.Version))
			{
				Probability = reader.ReadSingle();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TimeName, Time);
			node.Add(CountCurveName, CountCurve.ExportYAML(container));
			node.Add(CycleCountName, CycleCount);
			node.Add(RepeatIntervalName, RepeatInterval);
			if (HasProbability(container.ExportVersion))
			{
				node.Add(ProbabilityName, Probability);
			}
			return node;
		}

		public float Time { get; set; }
		public int CycleCount { get; set; }
		public float RepeatInterval { get; set; }
		public float Probability { get; set; }

		public const string TimeName = "time";
		public const string CountCurveName = "countCurve";
		public const string CycleCountName = "cycleCount";
		public const string RepeatIntervalName = "repeatInterval";
		public const string ProbabilityName = "probability";

		public MinMaxCurve CountCurve;
	}
}
