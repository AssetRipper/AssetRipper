using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.LightProbess
{
	public struct LightProbeOcclusion : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadOcclusionMaskChannel(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		public void Read(AssetStream stream)
		{
			m_probeOcclusionLightIndex = stream.ReadInt32Array();
			m_occlusion = stream.ReadSingleArray();
			if(IsReadOcclusionMaskChannel(stream.Version))
			{
				m_occlusionMaskChannel = stream.ReadByteArray();
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_ProbeOcclusionLightIndex", ProbeOcclusionLightIndex.ExportYAML(true));
			node.Add("m_Occlusion", Occlusion.ExportYAML());
			node.Add("m_OcclusionMaskChannel", OcclusionMaskChannel.ExportYAML());
			return node;
		}

		/// <summary>
		/// BakedLightIndex previously
		/// </summary>
		public IReadOnlyList<int> ProbeOcclusionLightIndex => m_probeOcclusionLightIndex;
		public IReadOnlyList<float> Occlusion => m_occlusion;
		public IReadOnlyList<byte> OcclusionMaskChannel => m_occlusionMaskChannel;

		private int[] m_probeOcclusionLightIndex;
		private float[] m_occlusion;
		private byte[] m_occlusionMaskChannel;
	}
}
