using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.LightmapSettingss
{
	public struct GISettings : IAssetReadable, IYAMLExportable
	{
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

		public void Read(AssetStream stream)
		{
			BounceScale = stream.ReadSingle();
			IndirectOutputScale = stream.ReadSingle();
			AlbedoBoost = stream.ReadSingle();
			TemporalCoherenceThreshold = stream.ReadSingle();
			EnvironmentLightingMode = stream.ReadUInt32();
			EnableBakedLightmaps = stream.ReadBoolean();
			EnableRealtimeLightmaps = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
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
