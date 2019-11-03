using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightProbess
{
	public struct LightProbeOcclusion : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasOcclusionMaskChannel(Version version) => version.IsGreaterEqual(5, 6);

		public void Read(AssetReader reader)
		{
			ProbeOcclusionLightIndex = reader.ReadInt32Array();
			Occlusion = reader.ReadSingleArray();
			if (HasOcclusionMaskChannel(reader.Version))
			{
				OcclusionMaskChannel = reader.ReadByteArray();
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
		public int[] ProbeOcclusionLightIndex { get; set; }
		public float[] Occlusion { get; set; }
		public byte[] OcclusionMaskChannel { get; set; }

		public const string ProbeOcclusionLightIndexName = "m_ProbeOcclusionLightIndex";
		public const string OcclusionName = "m_Occlusion";
		public const string OcclusionMaskChannelName = "m_OcclusionMaskChannel";
	}
}
