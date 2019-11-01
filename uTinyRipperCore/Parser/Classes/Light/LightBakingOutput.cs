using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Lights
{
	public struct LightBakingOutput : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 2017.3
		/// </summary>
		public static bool IsReadLightmappingMask(Version version)
		{
			return version.IsLess(2017, 3);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadIsBaked(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		
		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2017, 3))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			ProbeOcclusionLightIndex = reader.ReadInt32();
			OcclusionMaskChannel = reader.ReadInt32();
			if (IsReadLightmappingMask(reader.Version))
			{
				LightmappingMask = reader.ReadInt32();
			}
			if (IsReadIsBaked(reader.Version))
			{
				LightmapBakeMode.Read(reader);
				IsBaked = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(ProbeOcclusionLightIndexName, ProbeOcclusionLightIndex);
			node.Add(OcclusionMaskChannelName, OcclusionMaskChannel);
			if (GetSerializedVersion(container.Version) >= 2)
			{
				node.Add(LightmapBakeModeName, LightmapBakeMode.ExportYAML(container));
				node.Add(IsBakedName, IsBaked);
			}
			else
			{
				node.Add(LightmappingMaskName, LightmappingMask);
			}
			return node;
		}

		public int ProbeOcclusionLightIndex { get; private set; }
		public int OcclusionMaskChannel { get; private set; }
		public int LightmappingMask { get; private set; }
		public bool IsBaked { get; private set; }

		public const string ProbeOcclusionLightIndexName = "probeOcclusionLightIndex";
		public const string OcclusionMaskChannelName = "occlusionMaskChannel";
		public const string LightmapBakeModeName = "lightmapBakeMode";
		public const string IsBakedName = "isBaked";
		public const string LightmappingMaskName = "lightmappingMask";

		public LightmapBakeMode LightmapBakeMode;
	}
}
