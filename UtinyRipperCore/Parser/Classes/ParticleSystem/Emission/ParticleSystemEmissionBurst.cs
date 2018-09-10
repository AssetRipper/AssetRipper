using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct ParticleSystemEmissionBurst : IAssetReadable, IYAMLExportable
	{
		public ParticleSystemEmissionBurst(float time, int minValue, int maxValue)
		{
			Time = time;
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
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("time", Time);
			node.Add("countCurve", CountCurve.ExportYAML(container));
			node.Add("cycleCount", CycleCount);
			node.Add("repeatInterval", RepeatInterval);
			return node;
		}

		public float Time { get; private set; }
		public int CycleCount { get; private set; }
		public float RepeatInterval { get; private set; }

		public MinMaxCurve CountCurve;
	}
}
