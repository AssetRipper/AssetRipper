using uTinyRipper.AssetExporters;
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

		public static bool IsReadResolution(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		public static bool IsReadIrradianceQuality(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		public static bool IsReadEnvironmentResolution(Version version)
		{
			// unknown version
			return version.IsEqual(5, 0, 0, VersionType.Beta);
		}
		public static bool IsReadIsTransparent(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		public static bool IsReadEdgeStitching(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.0.1 and greater
		/// </summary>
		public static bool IsReadPushoff(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		public static bool IsReadBakedLightmapTag(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadLimitLightmapCount(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		public static bool IsReadAOQuality(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool IsReadPushoffFirst(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 5.0.1 and greater
		/// </summary>
		private static bool IsReadBakedLightmapTagFirst(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}

		private static int GetSerializedVersion(Version version)
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadResolution(reader.Version))
			{
				Resolution = reader.ReadSingle();
			}
			ClusterResolution = reader.ReadSingle();
			IrradianceBudget = reader.ReadInt32();
			if (IsReadIrradianceQuality(reader.Version))
			{
				IrradianceQuality = reader.ReadInt32();
			}
			BackFaceTolerance = reader.ReadSingle();
			if (IsReadEnvironmentResolution(reader.Version))
			{
				EnvironmentResolution = reader.ReadInt32();
			}
			if (IsReadIsTransparent(reader.Version))
			{
				IsTransparent = reader.ReadInt32();
				ModellingTolerance = reader.ReadSingle();
			}
			SystemTag = reader.ReadInt32();
			if (IsReadEdgeStitching(reader.Version))
			{
				EdgeStitching = reader.ReadInt32();
				BlurRadius = reader.ReadInt32();
				DirectLightQuality = reader.ReadInt32();
				AntiAliasingSamples = reader.ReadInt32();
			}
			if (IsReadPushoff(reader.Version))
			{
				if (IsReadPushoffFirst(reader.Version))
				{
					Pushoff = reader.ReadSingle();
				}
			}
			if (IsReadBakedLightmapTag(reader.Version))
			{
				if (IsReadBakedLightmapTagFirst(reader.Version))
				{
					BakedLightmapTag = reader.ReadInt32();
				}
			}
			if (IsReadPushoff(reader.Version))
			{
				if (!IsReadPushoffFirst(reader.Version))
				{
					Pushoff = reader.ReadSingle();
				}
			}
			if (IsReadLimitLightmapCount(reader.Version))
			{
				LimitLightmapCount = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);

				MaxLightmapCount = reader.ReadInt32();
			}
			if (IsReadAOQuality(reader.Version))
			{
				AOQuality = reader.ReadInt32();
				AOAntiAliasingSamples = reader.ReadInt32();
			}
			if (IsReadBakedLightmapTag(reader.Version))
			{
				if (!IsReadBakedLightmapTagFirst(reader.Version))
				{
					BakedLightmapTag = reader.ReadInt32();
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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
			if (IsReadLimitLightmapCount(container.ExportVersion))
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
			return IsReadResolution(version) ? Resolution : 1.0f;
		}
		private int GetIrradianceQuality(Version version)
		{
			return IsReadIrradianceQuality(version) ? IrradianceQuality : 8192;
		}
		private float GetModellingTolerance(Version version)
		{
			return IsReadIsTransparent(version) ? ModellingTolerance : 0.001f;
		}
		private int GetEdgeStitching(Version version)
		{
			return IsReadEdgeStitching(version) ? EdgeStitching : 1;
		}
		private int GetBlurRadius(Version version)
		{
			return IsReadEdgeStitching(version) ? BlurRadius : 2;
		}
		private int GetDirectLightQuality(Version version)
		{
			return IsReadEdgeStitching(version) ? DirectLightQuality : 64;
		}
		private int GetAntiAliasingSamples(Version version)
		{
			return IsReadEdgeStitching(version) ? AntiAliasingSamples : 8;
		}
		private int GetBakedLightmapTag(Version version)
		{
			return IsReadEdgeStitching(version) ? BakedLightmapTag : -1;
		}
		private float GetPushoff(Version version)
		{
			return IsReadEdgeStitching(version) ? Pushoff : 0.0001f;
		}
		private int GetMaxLightmapCount(Version version)
		{
			return IsReadLimitLightmapCount(version) ? MaxLightmapCount : 1;
		}
		private int GetAOQuality(Version version)
		{
			return IsReadEdgeStitching(version) ? AOQuality : 256;
		}
		private int GetAOAntiAliasingSamples(Version version)
		{
			return IsReadEdgeStitching(version) ? AOAntiAliasingSamples : 16;
		}

		public override string ExportExtension => "giparams";

		public float Resolution { get; private set; }
		/// <summary>
		/// ClusterSize previously
		/// </summary>
		public float ClusterResolution { get; private set; }
		/// <summary>
		/// IrradBudget previously
		/// </summary>
		public int IrradianceBudget { get; private set; }
		public int IrradianceQuality { get; private set; }
		public float BackFaceTolerance { get; private set; }
		public int EnvironmentResolution { get; private set; }
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
