using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Light
{
	public sealed class LightBakingOutput : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(2017, 3))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 2017.3
		/// </summary>
		public static bool HasLightmappingMask(UnityVersion version) => version.IsLess(2017, 3);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasIsBaked(UnityVersion version) => version.IsGreaterEqual(2017, 3);

		public void Read(AssetReader reader)
		{
			ProbeOcclusionLightIndex = reader.ReadInt32();
			OcclusionMaskChannel = reader.ReadInt32();
			if (HasLightmappingMask(reader.Version))
			{
				LightmappingMask = reader.ReadInt32();
			}
			if (HasIsBaked(reader.Version))
			{
				LightmapBakeMode.Read(reader);
				IsBaked = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ProbeOcclusionLightIndexName, ProbeOcclusionLightIndex);
			node.Add(OcclusionMaskChannelName, OcclusionMaskChannel);
			if (ToSerializedVersion(container.Version) >= 2)
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

		public int ProbeOcclusionLightIndex { get; set; }
		public int OcclusionMaskChannel { get; set; }
		public int LightmappingMask { get; set; }
		public bool IsBaked { get; set; }

		public const string ProbeOcclusionLightIndexName = "probeOcclusionLightIndex";
		public const string OcclusionMaskChannelName = "occlusionMaskChannel";
		public const string LightmapBakeModeName = "lightmapBakeMode";
		public const string IsBakedName = "isBaked";
		public const string LightmappingMaskName = "lightmappingMask";

		public LightmapBakeMode LightmapBakeMode = new();
	}
}
