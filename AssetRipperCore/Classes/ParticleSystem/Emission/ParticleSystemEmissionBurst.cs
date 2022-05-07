using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem.Emission
{
	public sealed class ParticleSystemEmissionBurst : IAssetReadable, IYamlExportable
	{
		public ParticleSystemEmissionBurst() { }
		public ParticleSystemEmissionBurst(float time, int minValue, int maxValue)
		{
			Time = time;
			CycleCount = 1;
			RepeatInterval = 0.01f;
			CountCurve = new MinMaxCurve(minValue, maxValue);
			Probability = 1.0f;
		}

		public static int ToSerializedVersion(UnityVersion version)
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
		public static bool HasCurve(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasProbability(UnityVersion version) => version.IsGreaterEqual(2018, 3);

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TimeName, Time);
			node.Add(CountCurveName, CountCurve.ExportYaml(container));
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

		public MinMaxCurve CountCurve = new();
	}
}
