using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Lights
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(2017, 3))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetStream stream)
		{
			ProbeOcclusionLightIndex = stream.ReadInt32();
			OcclusionMaskChannel = stream.ReadInt32();
			if (IsReadLightmappingMask(stream.Version))
			{
				LightmappingMask = stream.ReadInt32();
			}
			if (IsReadIsBaked(stream.Version))
			{
				LightmapBakeMode.Read(stream);
				IsBaked = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("probeOcclusionLightIndex", ProbeOcclusionLightIndex);
			node.Add("occlusionMaskChannel", OcclusionMaskChannel);
			if (GetSerializedVersion(container.Version) >= 2)
			{
				node.Add("lightmapBakeMode", LightmapBakeMode.ExportYAML(container));
				node.Add("isBaked", IsBaked);
			}
			else
			{
				node.Add("lightmappingMask", LightmappingMask);
			}
			return node;
		}

		public int ProbeOcclusionLightIndex { get; private set; }
		public int OcclusionMaskChannel { get; private set; }
		public int LightmappingMask { get; private set; }
		public bool IsBaked { get; private set; }

		public LightmapBakeMode LightmapBakeMode;
	}
}
