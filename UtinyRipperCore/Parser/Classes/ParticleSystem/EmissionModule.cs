using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct EmissionModule : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			RateOverTime.Read(stream);
			RateOverDistance.Read(stream);
			BurstCount = stream.ReadInt32();
			stream.AlignStream(AlignType.Align4);
			
			m_bursts = stream.ReadArray<ParticleSystemEmissionBurst>();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
			node.Add("rateOverTime", RateOverTime.ExportYAML(exporter));
			node.Add("rateOverDistance", RateOverDistance.ExportYAML(exporter));
			node.Add("m_BurstCount", BurstCount);
			node.Add("m_Bursts", Bursts.ExportYAML(exporter));
			return node;
		}

		public bool Enabled { get; private set; }
		public int BurstCount { get; private set; }
		public IReadOnlyList<ParticleSystemEmissionBurst> Bursts => m_bursts;

		public MinMaxCurve RateOverTime;
		public MinMaxCurve RateOverDistance;

		private ParticleSystemEmissionBurst[] m_bursts;
	}
}
