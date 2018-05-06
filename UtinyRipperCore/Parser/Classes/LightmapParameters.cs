using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class LightmapParameters : NamedObject
	{
		public LightmapParameters(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

			/*if (version.IsGreaterEqual())
			{
				return 3;
			}
			if (version.IsGreaterEqual())
			{
				return 2;
			}
			return 1;*/
			throw new NotImplementedException();
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Resolution = stream.ReadSingle();
			ClusterResolution = stream.ReadSingle();
			IrradianceBudget = stream.ReadInt32();
			IrradianceQuality = stream.ReadInt32();
			BackFaceTolerance = stream.ReadSingle();
			IsTransparent = stream.ReadInt32();
			ModellingTolerance = stream.ReadSingle();
			SystemTag = stream.ReadInt32();
			EdgeStitching = stream.ReadInt32();
			BlurRadius = stream.ReadInt32();
			DirectLightQuality = stream.ReadInt32();
			AntiAliasingSamples = stream.ReadInt32();
			BakedLightmapTag = stream.ReadInt32();
			Pushoff = stream.ReadSingle();
			AOQuality = stream.ReadInt32();
			AOAntiAliasingSamples = stream.ReadInt32();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("resolution", Resolution);
			node.Add("clusterResolution", ClusterResolution);
			node.Add("irradianceBudget", IrradianceBudget);
			node.Add("irradianceQuality", IrradianceQuality);
			node.Add("backFaceTolerance", BackFaceTolerance);
			node.Add("isTransparent", IsTransparent);
			node.Add("modellingTolerance", ModellingTolerance);
			node.Add("systemTag", SystemTag);
			node.Add("edgeStitching", EdgeStitching);
			node.Add("blurRadius", BlurRadius);
			node.Add("directLightQuality", DirectLightQuality);
			node.Add("antiAliasingSamples", AntiAliasingSamples);
			node.Add("bakedLightmapTag", BakedLightmapTag);
			node.Add("pushoff", Pushoff);
			node.Add("AOQuality", AOQuality);
			node.Add("AOAntiAliasingSamples", AOAntiAliasingSamples);
			return node;
		}

		public override string ExportExtension => "giparams";

		public float Resolution { get; private set; }
		public float ClusterResolution { get; private set; }
		public int IrradianceBudget { get; private set; }
		public int IrradianceQuality { get; private set; }
		public float BackFaceTolerance { get; private set; }
		public int IsTransparent { get; private set; }
		public float ModellingTolerance { get; private set; }
		public int SystemTag { get; private set; }
		public int EdgeStitching { get; private set; }
		public int BlurRadius { get; private set; }
		public int DirectLightQuality { get; private set; }
		public int AntiAliasingSamples { get; private set; }
		public int BakedLightmapTag { get; private set; }
		public float Pushoff { get; private set; }
		public int AOQuality { get; private set; }
		public int AOAntiAliasingSamples { get; private set; }
	}
}
