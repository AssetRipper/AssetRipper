using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct ParticleSystemEmissionBurst : IAssetReadable, IYAMLExportable
	{
		public ParticleSystemEmissionBurst(float time, int minValue, int maxValue)
		{
			Time = time;
			MinValue = minValue;
			MaxValue = maxValue;
			CycleCount = 1;
			RepeatInterval = 0.01f;
			CountCurve = new MinMaxCurve(minValue, maxValue);
		}

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadCurve(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}
			
			if (version.IsGreaterEqual(2017, 2))
			{
				return 2;
			}
			return 1;
		}

		private MinMaxCurve GetExportCountCurve(Version version)
		{
			return IsReadCurve(version) ? CountCurve : new MinMaxCurve(MinValue, MaxValue);
		}

		public void Read(AssetStream stream)
		{
			Time = stream.ReadSingle();
			if (IsReadCurve(stream.Version))
			{
				CountCurve.Read(stream);
			}
			else
			{
				MinValue = stream.ReadInt32();
				MaxValue = stream.ReadInt32();
			}
			CycleCount = stream.ReadInt32();
			RepeatInterval = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("time", Time);
			node.Add("countCurve", GetExportCountCurve(container.Version).ExportYAML(container));
			node.Add("cycleCount", CycleCount);
			node.Add("repeatInterval", RepeatInterval);
			return node;
		}

		public float Time { get; private set; }
		public int MinValue { get; private set; }
		public int MaxValue { get; private set; }
		public int CycleCount { get; private set; }
		public float RepeatInterval { get; private set; }

		public MinMaxCurve CountCurve;
	}
}
