using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.LightProbes
{
	public sealed class LightProbeOcclusion : IAssetReadable, IYamlExportable
	{
		/// <summary>
		/// 5.6.0b2 and greater
		/// </summary>
		public static bool HasOcclusionMaskChannel(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 2);

		public void Read(AssetReader reader)
		{
			ProbeOcclusionLightIndex = reader.ReadInt32Array();
			Occlusion = reader.ReadSingleArray();
			if (HasOcclusionMaskChannel(reader.Version))
			{
				OcclusionMaskChannel = reader.ReadByteArray();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ProbeOcclusionLightIndexName, ProbeOcclusionLightIndex.ExportYaml(true));
			node.Add(OcclusionName, Occlusion.ExportYaml());
			if (HasOcclusionMaskChannel(container.ExportVersion))
			{
				node.Add(OcclusionMaskChannelName, OcclusionMaskChannel.ExportYaml());
			}

			return node;
		}

		/// <summary>
		/// BakedLightIndex previously (before 5.6.0b2)
		/// </summary>
		public int[] ProbeOcclusionLightIndex { get; set; }
		public float[] Occlusion { get; set; }
		/// <summary>
		/// ShadowMaskChannel previously (before 5.6.0b5)
		/// </summary>
		public byte[] OcclusionMaskChannel { get; set; } = System.Array.Empty<byte>();

		public const string BakedLightIndexName = "m_BakedLightIndex";
		public const string ProbeOcclusionLightIndexName = "m_ProbeOcclusionLightIndex";
		public const string OcclusionName = "m_Occlusion";
		public const string ShadowMaskChannelName = "m_ShadowMaskChannel";
		public const string OcclusionMaskChannelName = "m_OcclusionMaskChannel";
	}
}
