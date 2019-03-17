using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightProbess
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

		public void Read(AssetReader reader)
		{
			m_probeOcclusionLightIndex = reader.ReadInt32Array();
			m_occlusion = reader.ReadSingleArray();
			if(IsReadOcclusionMaskChannel(reader.Version))
			{
				m_occlusionMaskChannel = reader.ReadByteArray();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
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
