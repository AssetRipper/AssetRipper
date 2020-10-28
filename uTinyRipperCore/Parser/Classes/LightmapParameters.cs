using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// EnlightenSystemBuildParameters previously
	/// </summary>
	public sealed class LightmapParameters : NamedObject
	{
		public LightmapParameters(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// Default value for ModellingTolerance has been changed from 0.001 to 0.01
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 3;
			}

			// Default value for EdgeStitching has been changed from 1 to 1 (what?)
			// return 2;
			return 1;
		}

		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasResolution(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasIrradianceQuality(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasEnvironmentResolution(Version version) => version.IsEqual(5, 0, 0, VersionType.Beta);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasIsTransparent(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasEdgeStitching(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.0.0p2 and greater
		/// </summary>
		public static bool HasPushoff(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Patch, 2);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasBakedLightmapTag(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasLimitLightmapCount(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasAOQuality(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool IsPushoffFirst(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 5.0.0p2 and greater
		/// </summary>
		private static bool IsBakedLightmapTagFirst(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Patch, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasResolution(reader.Version))
			{
				Resolution = reader.ReadSingle();
			}
			ClusterResolution = reader.ReadSingle();
			IrradianceBudget = reader.ReadInt32();
			if (HasIrradianceQuality(reader.Version))
			{
				IrradianceQuality = reader.ReadInt32();
			}
			BackFaceTolerance = reader.ReadSingle();
			if (HasEnvironmentResolution(reader.Version))
			{
				EnvironmentResolution = reader.ReadInt32();
			}
			if (HasIsTransparent(reader.Version))
			{
				IsTransparent = reader.ReadInt32();
				ModellingTolerance = reader.ReadSingle();
			}
			SystemTag = reader.ReadInt32();
			if (HasEdgeStitching(reader.Version))
			{
				EdgeStitching = reader.ReadInt32();
				BlurRadius = reader.ReadInt32();
				DirectLightQuality = reader.ReadInt32();
				AntiAliasingSamples = reader.ReadInt32();
			}
			if (HasPushoff(reader.Version))
			{
				if (IsPushoffFirst(reader.Version))
				{
					Pushoff = reader.ReadSingle();
				}
			}
			if (HasBakedLightmapTag(reader.Version))
			{
				if (IsBakedLightmapTagFirst(reader.Version))
				{
					BakedLightmapTag = reader.ReadInt32();
				}
			}
			if (HasPushoff(reader.Version))
			{
				if (!IsPushoffFirst(reader.Version))
				{
					Pushoff = reader.ReadSingle();
				}
			}
			if (HasLimitLightmapCount(reader.Version))
			{
				LimitLightmapCount = reader.ReadBoolean();
				reader.AlignStream();

				MaxLightmapCount = reader.ReadInt32();
			}
			if (HasAOQuality(reader.Version))
			{
				AOQuality = reader.ReadInt32();
				AOAntiAliasingSamples = reader.ReadInt32();
			}
			if (HasBakedLightmapTag(reader.Version))
			{
				if (!IsBakedLightmapTagFirst(reader.Version))
				{
					BakedLightmapTag = reader.ReadInt32();
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ResolutionName, GetResolution(container.Version));
			node.Add(ClusterResolutionName, ClusterResolution);
			node.Add(IrradianceBudgetName, IrradianceBudget);
			node.Add(IrradianceQualityName, GetIrradianceQuality(container.Version));
			node.Add(BackFaceToleranceName, BackFaceTolerance);
			node.Add(IsTransparentName, IsTransparent);
			node.Add(ModellingToleranceName, GetModellingTolerance(container.Version));
			node.Add(SystemTagName, SystemTag);
			node.Add(EdgeStitchingName, GetEdgeStitching(container.Version));
			node.Add(BlurRadiusName, GetBlurRadius(container.Version));
			node.Add(DirectLightQualityName, GetDirectLightQuality(container.Version));
			node.Add(AntiAliasingSamplesName, GetAntiAliasingSamples(container.Version));
			node.Add(BakedLightmapTagName, GetBakedLightmapTag(container.Version));
			node.Add(PushoffName, GetPushoff(container.Version));
			if (HasLimitLightmapCount(container.ExportVersion))
			{
				node.Add(LimitLightmapCountName, LimitLightmapCount);
				node.Add(MaxLightmapCountName, GetMaxLightmapCount(container.Version));
			}
			node.Add(AOQualityName, GetAOQuality(container.Version));
			node.Add(AOAntiAliasingSamplesName, GetAOAntiAliasingSamples(container.Version));
			return node;
		}

		private float GetResolution(Version version)
		{
			return HasResolution(version) ? Resolution : 1.0f;
		}
		private int GetIrradianceQuality(Version version)
		{
			return HasIrradianceQuality(version) ? IrradianceQuality : 8192;
		}
		private float GetModellingTolerance(Version version)
		{
			return HasIsTransparent(version) ? ModellingTolerance : 0.001f;
		}
		private int GetEdgeStitching(Version version)
		{
			return HasEdgeStitching(version) ? EdgeStitching : 1;
		}
		private int GetBlurRadius(Version version)
		{
			return HasEdgeStitching(version) ? BlurRadius : 2;
		}
		private int GetDirectLightQuality(Version version)
		{
			return HasEdgeStitching(version) ? DirectLightQuality : 64;
		}
		private int GetAntiAliasingSamples(Version version)
		{
			return HasEdgeStitching(version) ? AntiAliasingSamples : 8;
		}
		private int GetBakedLightmapTag(Version version)
		{
			return HasEdgeStitching(version) ? BakedLightmapTag : -1;
		}
		private float GetPushoff(Version version)
		{
			return HasEdgeStitching(version) ? Pushoff : 0.0001f;
		}
		private int GetMaxLightmapCount(Version version)
		{
			return HasLimitLightmapCount(version) ? MaxLightmapCount : 1;
		}
		private int GetAOQuality(Version version)
		{
			return HasEdgeStitching(version) ? AOQuality : 256;
		}
		private int GetAOAntiAliasingSamples(Version version)
		{
			return HasEdgeStitching(version) ? AOAntiAliasingSamples : 16;
		}

		public override string ExportExtension => "giparams";

		public float Resolution { get; set; }
		/// <summary>
		/// ClusterSize previously
		/// </summary>
		public float ClusterResolution { get; set; }
		/// <summary>
		/// IrradBudget previously
		/// </summary>
		public int IrradianceBudget { get; set; }
		public int IrradianceQuality { get; set; }
		public float BackFaceTolerance { get; set; }
		public int EnvironmentResolution { get; set; }
		public int IsTransparent { get; set; }
		public float ModellingTolerance { get; set; }
		public int SystemTag { get; set; }
		public int EdgeStitching { get; set; }
		public int BlurRadius { get; set; }
		public int DirectLightQuality { get; set; }
		public int AntiAliasingSamples { get; set; }
		public int BakedLightmapTag { get; set; }
		public float Pushoff { get; set; }
		public bool LimitLightmapCount { get; set; }
		public int MaxLightmapCount { get; set; }
		public int AOQuality { get; set; }
		public int AOAntiAliasingSamples { get; set; }

		public const string ResolutionName = "resolution";
		public const string ClusterSizeName = "m_ClusterSize";
		public const string ClusterResolutionName = "clusterResolution";
		public const string IrradBudgetName = "m_IrradBudget";
		public const string IrradianceBudgetName = "irradianceBudget";
		public const string IrradianceQualityName = "irradianceQuality";
		public const string MBackFaceToleranceName = "m_BackFaceTolerance";
		public const string BackFaceToleranceName = "backFaceTolerance";
		public const string EnvironmentResolutionName = "m_EnvironmentResolution";
		public const string IsTransparentName = "isTransparent";
		public const string ModellingToleranceName = "modellingTolerance";
		public const string MSystemTagName = "m_SystemTag";
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
