using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Converters;
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
			node.Add(ProbeOcclusionLightIndexName, ProbeOcclusionLightIndex.ExportYAML(true));
			node.Add(OcclusionName, Occlusion.ExportYAML());
			node.Add(OcclusionMaskChannelName, OcclusionMaskChannel.ExportYAML());
			return node;
		}

		/// <summary>
		/// BakedLightIndex previously
		/// </summary>
		public IReadOnlyList<int> ProbeOcclusionLightIndex => m_probeOcclusionLightIndex;
		public IReadOnlyList<float> Occlusion => m_occlusion;
		public IReadOnlyList<byte> OcclusionMaskChannel => m_occlusionMaskChannel;

		public const string ProbeOcclusionLightIndexName = "m_ProbeOcclusionLightIndex";
		public const string OcclusionName = "m_Occlusion";
		public const string OcclusionMaskChannelName = "m_OcclusionMaskChannel";

		private int[] m_probeOcclusionLightIndex;
		private float[] m_occlusion;
		private byte[] m_occlusionMaskChannel;
	}
}
