using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

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

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

#warning unknown
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 1))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			BounceScale = reader.ReadSingle();
			IndirectOutputScale = reader.ReadSingle();
			AlbedoBoost = reader.ReadSingle();
			TemporalCoherenceThreshold = reader.ReadSingle();
			EnvironmentLightingMode = reader.ReadUInt32();
			EnableBakedLightmaps = reader.ReadBoolean();
			EnableRealtimeLightmaps = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_BounceScale", BounceScale);
			node.Add("m_IndirectOutputScale", IndirectOutputScale);
			node.Add("m_AlbedoBoost", AlbedoBoost);
			node.Add("m_TemporalCoherenceThreshold", TemporalCoherenceThreshold);
			node.Add("m_EnvironmentLightingMode", EnvironmentLightingMode);
			node.Add("m_EnableBakedLightmaps", EnableBakedLightmaps);
			node.Add("m_EnableRealtimeLightmaps", EnableRealtimeLightmaps);
			return node;
		}

		public float BounceScale { get; private set; }
		public float IndirectOutputScale { get; private set; }
		public float AlbedoBoost { get; private set; }
		public float TemporalCoherenceThreshold { get; private set; }
		public uint EnvironmentLightingMode { get; private set; }
		public bool EnableBakedLightmaps { get; private set; }
		public bool EnableRealtimeLightmaps { get; private set; }
	}
}
