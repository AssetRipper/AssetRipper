using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct GISettings : IAssetReadable, IYAMLExportable
	{
		public GISettings(bool _)
		{
			BounceScale = 1.0f;
			IndirectOutputScale = 1.0f;
			AlbedoBoost = 1.0f;
			TemporalCoherenceThreshold = 1.0f;
			EnvironmentLightingMode = 0;
			EnableBakedLightmaps = true;
			EnableRealtimeLightmaps = true;
		}
		
		/// <summary>
		/// Less than 2018.3
		/// </summary>
		public static bool IsReadTemporalCoherenceThreshold(Version version)
		{
			return version.IsLess(2018, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
			return 2;
			// unknown (5.0.0a) version
			//return 1;
		}

		public void Read(AssetReader reader)
		{
			BounceScale = reader.ReadSingle();
			IndirectOutputScale = reader.ReadSingle();
			AlbedoBoost = reader.ReadSingle();
			if (IsReadTemporalCoherenceThreshold(reader.Version))
			{
				TemporalCoherenceThreshold = reader.ReadSingle();
			}
			EnvironmentLightingMode = reader.ReadUInt32();
			EnableBakedLightmaps = reader.ReadBoolean();
			EnableRealtimeLightmaps = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(BounceScaleName, BounceScale);
			node.Add(IndirectOutputScaleName, IndirectOutputScale);
			node.Add(AlbedoBoostName, AlbedoBoost);
			if (IsReadTemporalCoherenceThreshold(container.ExportVersion))
			{
				node.Add(TemporalCoherenceThresholdName, TemporalCoherenceThreshold);
			}
			node.Add(EnvironmentLightingModeName, EnvironmentLightingMode);
			node.Add(EnableBakedLightmapsName, EnableBakedLightmaps);
			node.Add(EnableRealtimeLightmapsName, EnableRealtimeLightmaps);
			return node;
		}

		public float BounceScale { get; private set; }
		public float IndirectOutputScale { get; private set; }
		public float AlbedoBoost { get; private set; }
		public float TemporalCoherenceThreshold { get; private set; }
		public uint EnvironmentLightingMode { get; private set; }
		public bool EnableBakedLightmaps { get; private set; }
		public bool EnableRealtimeLightmaps { get; private set; }

		public const string BounceScaleName = "m_BounceScale";
		public const string IndirectOutputScaleName = "m_IndirectOutputScale";
		public const string AlbedoBoostName = "m_AlbedoBoost";
		public const string TemporalCoherenceThresholdName = "m_TemporalCoherenceThreshold";
		public const string EnvironmentLightingModeName = "m_EnvironmentLightingMode";
		public const string EnableBakedLightmapsName = "m_EnableBakedLightmaps";
		public const string EnableRealtimeLightmapsName = "m_EnableRealtimeLightmaps";
	}
}
