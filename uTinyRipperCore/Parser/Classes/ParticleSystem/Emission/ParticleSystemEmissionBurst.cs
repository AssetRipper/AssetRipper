using uTinyRipper.AssetExporters;
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

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadCurve(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadProbability(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2017, 2))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			if (IsReadCurve(reader.Version))
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
			if (IsReadProbability(reader.Version))
			{
				Probability = reader.ReadSingle();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TimeName, Time);
			node.Add(CountCurveName, CountCurve.ExportYAML(container));
			node.Add(CycleCountName, CycleCount);
			node.Add(RepeatIntervalName, RepeatInterval);
			if (IsReadProbability(container.ExportVersion))
			{
				node.Add(ProbabilityName, Probability);
			}
			return node;
		}

		public float Time { get; private set; }
		public int CycleCount { get; private set; }
		public float RepeatInterval { get; private set; }
		public float Probability { get; private set; }

		public const string TimeName = "time";
		public const string CountCurveName = "countCurve";
		public const string CycleCountName = "cycleCount";
		public const string RepeatIntervalName = "repeatInterval";
		public const string ProbabilityName = "probability";

		public MinMaxCurve CountCurve;
	}
}
