using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class LightmapParameters : NamedObject
	{
		public LightmapParameters(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadLimitLightmapCount(Version version)
		{
			return version.IsGreaterEqual(2019);
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool IsReadPushoffFirst(Version version)
		{
			return version.IsGreaterEqual(2019);
		}

		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 3;
			/*if (version.IsGreaterEqual())
			{
				return 3;
			}
			if (version.IsGreaterEqual())
			{
				return 2;
			}
			return 1;*/
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Resolution = reader.ReadSingle();
			ClusterResolution = reader.ReadSingle();
			IrradianceBudget = reader.ReadInt32();
			IrradianceQuality = reader.ReadInt32();
			BackFaceTolerance = reader.ReadSingle();
			IsTransparent = reader.ReadInt32();
			ModellingTolerance = reader.ReadSingle();
			SystemTag = reader.ReadInt32();
			EdgeStitching = reader.ReadInt32();
			BlurRadius = reader.ReadInt32();
			DirectLightQuality = reader.ReadInt32();
			AntiAliasingSamples = reader.ReadInt32();
			if (IsReadPushoffFirst(reader.Version))
			{
				Pushoff = reader.ReadSingle();
			}
			BakedLightmapTag = reader.ReadInt32();
			if (!IsReadPushoffFirst(reader.Version))
			{
				Pushoff = reader.ReadSingle();
			}
			if (IsReadLimitLightmapCount(reader.Version))
			{
				LimitLightmapCount = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);

				MaxLightmapCount = reader.ReadInt32();
			}
			AOQuality = reader.ReadInt32();
			AOAntiAliasingSamples = reader.ReadInt32();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(ResolutionName, Resolution);
			node.Add(ClusterResolutionName, ClusterResolution);
			node.Add(IrradianceBudgetName, IrradianceBudget);
			node.Add(IrradianceQualityName, IrradianceQuality);
			node.Add(BackFaceToleranceName, BackFaceTolerance);
			node.Add(IsTransparentName, IsTransparent);
			node.Add(ModellingToleranceName, ModellingTolerance);
			node.Add(SystemTagName, SystemTag);
			node.Add(EdgeStitchingName, EdgeStitching);
			node.Add(BlurRadiusName, BlurRadius);
			node.Add(DirectLightQualityName, DirectLightQuality);
			node.Add(AntiAliasingSamplesName, AntiAliasingSamples);
			node.Add(BakedLightmapTagName, BakedLightmapTag);
			node.Add(PushoffName, Pushoff);
			if (IsReadLimitLightmapCount(container.ExportVersion))
			{
				node.Add(LimitLightmapCountName, LimitLightmapCount);
				node.Add(MaxLightmapCountName, GetMaxLightmapCount(container.Version));
			}
			node.Add(AOQualityName, AOQuality);
			node.Add(AOAntiAliasingSamplesName, AOAntiAliasingSamples);
			return node;
		}

		private int GetMaxLightmapCount(Version version)
		{
			return IsReadLimitLightmapCount(version) ? MaxLightmapCount : 1;
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
		public bool LimitLightmapCount { get; private set; }
		public int MaxLightmapCount { get; private set; }
		public int AOQuality { get; private set; }
		public int AOAntiAliasingSamples { get; private set; }

		public const string ResolutionName = "resolution";
		public const string ClusterResolutionName = "clusterResolution";
		public const string IrradianceBudgetName = "irradianceBudget";
		public const string IrradianceQualityName = "irradianceQuality";
		public const string BackFaceToleranceName = "backFaceTolerance";
		public const string IsTransparentName = "isTransparent";
		public const string ModellingToleranceName = "modellingTolerance";
		public const string SystemTagName = "systemTag";
		public const string EdgeStitchingName = "edgeStitching";
		public const string BlurRadiusName = "blurRadius";
		public const string DirectLightQualityName = "directLightQuality";
		public const string AntiAliasingSamplesName = "antiAliasingSamples";
		public const string BakedLightmapTagName = "bakedLightmapTag";
		public const string PushoffName = "pushoff";
		public const string LimitLightmapCountName = "limitLightmapCount";
		public const string MaxLightmapCountName = "maxLightmapCount";
		public const string AOQualityName = "AOQuality";
		public const string AOAntiAliasingSamplesName = "AOAntiAliasingSamples";
	}
}
