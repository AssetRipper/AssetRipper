using System.Collections.Generic;
using uTinyRipper.Classes.Physics2DSettingss;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class Physics2DSettings : GlobalGameManager
	{
		public Physics2DSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private Physics2DSettings(AssetInfo assetInfo, bool _) :
			base(assetInfo)
		{
			Gravity = new Vector2f(0.0f, -9.81f);
			VelocityIterations = 8;
			PositionIterations = 3;
			QueriesHitTriggers = true;
			LayerCollisionMatrix = new uint[32];
			for(int i = 0; i < LayerCollisionMatrix.Length; i++)
			{
				LayerCollisionMatrix[i] = uint.MaxValue;
			}
		}

		public static Physics2DSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new Physics2DSettings(assetInfo, true));
		}

		public static int ToSerializedVersion(Version version)
		{
			// disabled RaycastsHitTriggers backward compatibility?
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 7))
			{
				return 3;
			}
			// RaycastsHitTriggers renamed to QueriesHitTriggers, RaycastsStartInColliders renamed to QueriesStartInColliders
			if (version.IsGreaterEqual(5, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasPhysics2DSettings(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasVelocityThreshold(Version version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 4.6.1 to 5.6.0b6
		/// </summary>
		public static bool HasMinPenetrationForPenalty(Version version) => version.IsGreaterEqual(4, 6, 1) && version.IsLessEqual(5, 6, 0, VersionType.Beta, 6);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasBaumgarteScale(Version version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.6.0b7 and greater
		/// </summary>
		public static bool HasDefaultContactOffset(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 7);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasJobOptions(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasAutoSimulation(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		/// <summary>
		/// 4.6.1 and greater
		/// </summary>
		public static bool HasQueriesStartInColliders(Version version) => version.IsGreaterEqual(4, 6, 1);
		/// <summary>
		/// 4.5.3 to 4.6.0
		/// </summary>
		public static bool HasDeleteStopsCallbacks(Version version) => version.IsLessEqual(4, 6) && version.IsGreaterEqual(4, 5, 3);
		/// <summary>
		/// 4.6.1 to 2018.1 exclusive
		/// </summary>
		public static bool HasChangeStopsCallbacks(Version version) => version.IsGreaterEqual(4, 6, 1) && version.IsLess(2018);
		/// <summary>
		/// 5.6.0p1 and greater
		/// </summary>
		public static bool HasCallbacksOnDisable(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Patch);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasAutoSyncTransforms(Version version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool HasAlwaysShowColliders(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater and editor
		/// </summary>
		public static bool HasShowColliderAABB(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool HasContactArrowScale(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater and editor
		/// </summary>
		public static bool HasColliderAABBColor(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Gravity.Read(reader);
			DefaultMaterial.Read(reader);
			VelocityIterations = reader.ReadInt32();
			PositionIterations = reader.ReadInt32();
			if (HasVelocityThreshold(reader.Version))
			{
				VelocityThreshold = reader.ReadSingle();
				MaxLinearCorrection = reader.ReadSingle();
				MaxAngularCorrection = reader.ReadSingle();
				MaxTranslationSpeed = reader.ReadSingle();
				MaxRotationSpeed = reader.ReadSingle();
			}
			if (HasMinPenetrationForPenalty(reader.Version))
			{
				MinPenetrationForPenalty = reader.ReadSingle();
			}
			if (HasBaumgarteScale(reader.Version))
			{
				BaumgarteScale = reader.ReadSingle();
				BaumgarteTimeOfImpactScale = reader.ReadSingle();
				TimeToSleep = reader.ReadSingle();
				LinearSleepTolerance = reader.ReadSingle();
				AngularSleepTolerance = reader.ReadSingle();
			}
			if (HasDefaultContactOffset(reader.Version))
			{
				DefaultContactOffset = reader.ReadSingle();
			}
			if (HasJobOptions(reader.Version))
			{
				JobOptions.Read(reader);
			}
			if (HasAutoSimulation(reader.Version))
			{
				AutoSimulation = reader.ReadBoolean();
			}
			QueriesHitTriggers = reader.ReadBoolean();
			if (HasQueriesStartInColliders(reader.Version))
			{
				QueriesStartInColliders = reader.ReadBoolean();
			}
			if (HasDeleteStopsCallbacks(reader.Version))
			{
				DeleteStopsCallbacks = reader.ReadBoolean();
			}
			if (HasChangeStopsCallbacks(reader.Version))
			{
				ChangeStopsCallbacks = reader.ReadBoolean();
			}
			if (HasCallbacksOnDisable(reader.Version))
			{
				CallbacksOnDisable = reader.ReadBoolean();
			}
			if (HasAutoSyncTransforms(reader.Version))
			{
				AutoSyncTransforms = reader.ReadBoolean();
			}
#if UNIVERSAL
			if (HasAlwaysShowColliders(reader.Version, reader.Flags))
			{
				AlwaysShowColliders = reader.ReadBoolean();
				ShowColliderSleep = reader.ReadBoolean();
				ShowColliderContacts = reader.ReadBoolean();
			}
			if (HasShowColliderAABB(reader.Version, reader.Flags))
			{
				ShowColliderAABB = reader.ReadBoolean();
			}
#endif
			reader.AlignStream();

#if UNIVERSAL
			if (HasContactArrowScale(reader.Version, reader.Flags))
			{
				ContactArrowScale = reader.ReadSingle();
				ColliderAwakeColor.Read(reader);
				ColliderAsleepColor.Read(reader);
				ColliderContactColor.Read(reader);
			}
			if (HasColliderAABBColor(reader.Version, reader.Flags))
			{
				ColliderAABBColor.Read(reader);
			}
#endif
			LayerCollisionMatrix = reader.ReadUInt32Array();
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(DefaultMaterial, DefaultMaterialName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(GravityName, Gravity.ExportYAML(container));
			node.Add(DefaultMaterialName, DefaultMaterial.ExportYAML(container));
			node.Add(VelocityIterationsName, VelocityIterations);
			node.Add(PositionIterationsName, PositionIterations);
			node.Add(VelocityThresholdName, GetVelocityThreshold(container.Version));
			node.Add(MaxLinearCorrectionName, GetMaxLinearCorrection(container.Version));
			node.Add(MaxAngularCorrectionName, GetMaxAngularCorrection(container.Version));
			node.Add(MaxTranslationSpeedName, GetMaxTranslationSpeed(container.Version));
			node.Add(MaxRotationSpeedName, GetMaxRotationSpeed(container.Version));
			node.Add(BaumgarteScaleName, GetBaumgarteScale(container.Version));
			node.Add(BaumgarteTimeOfImpactScaleName, GetBaumgarteTimeOfImpactScale(container.Version));
			node.Add(TimeToSleepName, GetTimeToSleep(container.Version));
			node.Add(LinearSleepToleranceName, GetLinearSleepTolerance(container.Version));
			node.Add(AngularSleepToleranceName, GetAngularSleepTolerance(container.Version));
			node.Add(DefaultContactOffsetName, GetDefaultContactOffset(container.Version));
			// 2018
			//node.Add("m_JobOptions", GetJobOptions(container.Version));
			node.Add(AutoSimulationName, GetAutoSimulation(container.Version));
			node.Add(QueriesHitTriggersName, QueriesHitTriggers);
			node.Add(QueriesStartInCollidersName, GetQueriesStartInColliders(container.Version));
			node.Add(ChangeStopsCallbacksName, ChangeStopsCallbacks);
			node.Add(CallbacksOnDisableName, GetCallbacksOnDisable(container.Version));
			node.Add(AutoSyncTransformsName, GetAutoSyncTransforms(container.Version));
			node.Add(AlwaysShowCollidersName, GetAlwaysShowColliders());
			node.Add(ShowColliderSleepName, GetShowColliderSleep(container.Version, container.Flags));
			node.Add(ShowColliderContactsName, GetShowColliderContacts());
			node.Add(ShowColliderAABBName, GetShowColliderAABB());
			node.Add(ContactArrowScaleName, GetContactArrowScale(container.Version, container.Flags));
			node.Add(ColliderAwakeColorName, GetColliderAwakeColor(container.Version, container.Flags).ExportYAML(container));
			node.Add(ColliderAsleepColorName, GetColliderAsleepColor(container.Version, container.Flags).ExportYAML(container));
			node.Add(ColliderContactColorName, GetColliderContactColor(container.Version, container.Flags).ExportYAML(container));
			node.Add(ColliderAABBColorName, GetColliderAABBColor(container.Version, container.Flags).ExportYAML(container));
			node.Add(LayerCollisionMatrixName, LayerCollisionMatrix.ExportYAML(true));
			return node;
		}

		private float GetVelocityThreshold(Version version)
		{
			return HasVelocityThreshold(version) ? VelocityThreshold : 1.0f;
		}
		private float GetMaxLinearCorrection(Version version)
		{
			return HasVelocityThreshold(version) ? MaxLinearCorrection : 0.2f;
		}
		private float GetMaxAngularCorrection(Version version)
		{
			return HasVelocityThreshold(version) ? MaxAngularCorrection : 8.0f;
		}
		private float GetMaxTranslationSpeed(Version version)
		{
			return HasVelocityThreshold(version) ? MaxTranslationSpeed : 100.0f;
		}
		private float GetMaxRotationSpeed(Version version)
		{
			return HasVelocityThreshold(version) ? MaxRotationSpeed : 360.0f;
		}
		private float GetBaumgarteScale(Version version)
		{
			return HasBaumgarteScale(version) ? BaumgarteScale : 0.2f;
		}
		private float GetBaumgarteTimeOfImpactScale(Version version)
		{
			return HasBaumgarteScale(version) ? BaumgarteTimeOfImpactScale : 0.75f;
		}
		private float GetTimeToSleep(Version version)
		{
			return HasBaumgarteScale(version) ? TimeToSleep : 0.5f;
		}
		private float GetLinearSleepTolerance(Version version)
		{
			return HasBaumgarteScale(version) ? LinearSleepTolerance : 0.01f;
		}
		private float GetAngularSleepTolerance(Version version)
		{
			return HasBaumgarteScale(version) ? AngularSleepTolerance : 2.0f;
		}
		private float GetDefaultContactOffset(Version version)
		{
			return HasDefaultContactOffset(version) ? DefaultContactOffset : 0.01f;
		}
		private bool GetAutoSimulation(Version version)
		{
			return HasAutoSimulation(version) ? AutoSimulation : true;
		}
		private bool GetQueriesStartInColliders(Version version)
		{
			return HasQueriesStartInColliders(version) ? QueriesStartInColliders : true;
		}
		private bool GetCallbacksOnDisable(Version version)
		{
			return HasCallbacksOnDisable(version) ? CallbacksOnDisable : true;
		}
		private bool GetAutoSyncTransforms(Version version)
		{
			return HasAutoSyncTransforms(version) ? AutoSyncTransforms : true;
		}
		private bool GetAlwaysShowColliders()
		{
#if UNIVERSAL
			return AlwaysShowColliders;
#else
			return false;
#endif
		}
		private bool GetShowColliderSleep(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasAlwaysShowColliders(version, flags))
			{
				return ShowColliderSleep;
			}
#endif
			return true;
		}
		private bool GetShowColliderContacts()
		{
#if UNIVERSAL
			return ShowColliderContacts;
#else
			return false;
#endif
		}
		private bool GetShowColliderAABB()
		{
#if UNIVERSAL
			return ShowColliderAABB;
#else
			return false;
#endif
		}
		private float GetContactArrowScale(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasContactArrowScale(version, flags))
			{
				return ContactArrowScale;
			}
#endif
			return 0.2f;
		}
		private ColorRGBAf GetColliderAwakeColor(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasContactArrowScale(version, flags))
			{
				return ColliderAwakeColor;
			}
#endif
			return new ColorRGBAf(0.5686275f, 0.95686275f, 0.54509807f, 0.7529412f);
		}
		private ColorRGBAf GetColliderAsleepColor(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasContactArrowScale(version, flags))
			{
				return ColliderAsleepColor;
			}
#endif
			return new ColorRGBAf(0.5686275f, 0.95686275f, 0.54509807f, 0.36078432f);
		}
		private ColorRGBAf GetColliderContactColor(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasContactArrowScale(version, flags))
			{
				return ColliderContactColor;
			}
#endif
			return new ColorRGBAf(1.0f, 0.0f, 1.0f, 0.6862745f);
		}
		private ColorRGBAf GetColliderAABBColor(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasContactArrowScale(version, flags))
			{
				return ColliderAABBColor;
			}
#endif
			return new ColorRGBAf(1.0f, 1.0f, 0.0f, 0.2509804f);
		}

		public int VelocityIterations { get; set; }
		public int PositionIterations { get; set; }
		public float VelocityThreshold { get; set; }
		public float MaxLinearCorrection { get; set; }
		public float MaxAngularCorrection { get; set; }
		public float MaxTranslationSpeed { get; set; }
		public float MaxRotationSpeed { get; set; }
		public float MinPenetrationForPenalty { get; set; }
		public float BaumgarteScale { get; set; }
		public float BaumgarteTimeOfImpactScale { get; set; }
		public float TimeToSleep { get; set; }
		public float LinearSleepTolerance { get; set; }
		public float AngularSleepTolerance { get; set; }
		public float DefaultContactOffset { get; set; }
		public bool AutoSimulation { get; set; }
		/// <summary>
		/// RaycastsHitTriggers previously
		/// </summary>
		public bool QueriesHitTriggers { get; set; }
		/// <summary>
		/// RaycastsStartInColliders previously
		/// </summary>
		public bool QueriesStartInColliders { get; set; }
		public bool DeleteStopsCallbacks { get; set; }
		public bool ChangeStopsCallbacks { get; set; }
		public bool CallbacksOnDisable { get; set; }
		public bool AutoSyncTransforms { get; set; }
#if UNIVERSAL
		public bool AlwaysShowColliders { get; set; }
		public bool ShowColliderSleep { get; set; }
		public bool ShowColliderContacts { get; set; }
		public bool ShowColliderAABB { get; set; }
		public float ContactArrowScale { get; set; }
#endif
		public uint[] LayerCollisionMatrix { get; set; }

		public const string GravityName = "m_Gravity";
		public const string DefaultMaterialName = "m_DefaultMaterial";
		public const string VelocityIterationsName = "m_VelocityIterations";
		public const string PositionIterationsName = "m_PositionIterations";
		public const string VelocityThresholdName = "m_VelocityThreshold";
		public const string MaxLinearCorrectionName = "m_MaxLinearCorrection";
		public const string MaxAngularCorrectionName = "m_MaxAngularCorrection";
		public const string MaxTranslationSpeedName = "m_MaxTranslationSpeed";
		public const string MaxRotationSpeedName = "m_MaxRotationSpeed";
		public const string BaumgarteScaleName = "m_BaumgarteScale";
		public const string BaumgarteTimeOfImpactScaleName = "m_BaumgarteTimeOfImpactScale";
		public const string TimeToSleepName = "m_TimeToSleep";
		public const string LinearSleepToleranceName = "m_LinearSleepTolerance";
		public const string AngularSleepToleranceName = "m_AngularSleepTolerance";
		public const string DefaultContactOffsetName = "m_DefaultContactOffset";
		public const string AutoSimulationName = "m_AutoSimulation";
		public const string QueriesHitTriggersName = "m_QueriesHitTriggers";
		public const string QueriesStartInCollidersName = "m_QueriesStartInColliders";
		public const string ChangeStopsCallbacksName = "m_ChangeStopsCallbacks";
		public const string CallbacksOnDisableName = "m_CallbacksOnDisable";
		public const string AutoSyncTransformsName = "m_AutoSyncTransforms";
		public const string AlwaysShowCollidersName = "m_AlwaysShowColliders";
		public const string ShowColliderSleepName = "m_ShowColliderSleep";
		public const string ShowColliderContactsName = "m_ShowColliderContacts";
		public const string ShowColliderAABBName = "m_ShowColliderAABB";
		public const string ContactArrowScaleName = "m_ContactArrowScale";
		public const string ColliderAwakeColorName = "m_ColliderAwakeColor";
		public const string ColliderAsleepColorName = "m_ColliderAsleepColor";
		public const string ColliderContactColorName = "m_ColliderContactColor";
		public const string ColliderAABBColorName = "m_ColliderAABBColor";
		public const string LayerCollisionMatrixName = "m_LayerCollisionMatrix";

		public Vector2f Gravity;
		public PPtr<PhysicsMaterial2D> DefaultMaterial;
		public PhysicsJobOptions2D JobOptions;
#if UNIVERSAL
		public ColorRGBAf ColliderAwakeColor;
		public ColorRGBAf ColliderAsleepColor;
		public ColorRGBAf ColliderContactColor;
		public ColorRGBAf ColliderAABBColor;
#endif
	}
}
