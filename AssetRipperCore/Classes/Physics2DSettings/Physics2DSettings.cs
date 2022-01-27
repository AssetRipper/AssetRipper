using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Physics2DSettings
{
	public sealed class Physics2DSettings : GlobalGameManager
	{
		public Physics2DSettings(AssetInfo assetInfo) : base(assetInfo) { }

		private Physics2DSettings(AssetInfo assetInfo, bool _) : base(assetInfo)
		{
			Gravity = new Vector2f(0.0f, -9.81f);
			VelocityIterations = 8;
			PositionIterations = 3;
			QueriesHitTriggers = true;
			LayerCollisionMatrix = new uint[32];
			for (int i = 0; i < LayerCollisionMatrix.Length; i++)
			{
				LayerCollisionMatrix[i] = uint.MaxValue;
			}
		}

		public static Physics2DSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new Physics2DSettings(assetInfo, true));
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			// disabled RaycastsHitTriggers backward compatibility?
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 7))
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
		public static bool HasPhysics2DSettings(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasVelocityThreshold(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 4.6.1 to 5.6.0b6
		/// </summary>
		public static bool HasMinPenetrationForPenalty(UnityVersion version) => version.IsGreaterEqual(4, 6, 1) && version.IsLessEqual(5, 6, 0, UnityVersionType.Beta, 6);
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasBaumgarteScale(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.6.0b7 and greater
		/// </summary>
		public static bool HasDefaultContactOffset(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 7);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasJobOptions(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2017.1.0b2 and greater but less than 2020
		/// </summary>
		public static bool HasAutoSimulation(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2) && version.IsLess(2020);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasSimulationMode(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 4.6.1 and greater
		/// </summary>
		public static bool HasQueriesStartInColliders(UnityVersion version) => version.IsGreaterEqual(4, 6, 1);
		/// <summary>
		/// 4.5.3 to 4.6.0
		/// </summary>
		public static bool HasDeleteStopsCallbacks(UnityVersion version) => version.IsLessEqual(4, 6) && version.IsGreaterEqual(4, 5, 3);
		/// <summary>
		/// 4.6.1 to 2018.1 exclusive
		/// </summary>
		public static bool HasChangeStopsCallbacks(UnityVersion version) => version.IsGreaterEqual(4, 6, 1) && version.IsLess(2018);
		/// <summary>
		/// 5.6.0p1 and greater
		/// </summary>
		public static bool HasCallbacksOnDisable(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Patch);
		/// <summary>
		/// At least 2019.4 and later.
		/// </summary>
#warning This might be present in earlier versions. Check.
		public static bool HasReuseCollisionCallbacks(UnityVersion version) => version.IsGreaterEqual(2019, 4);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasAutoSyncTransforms(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool HasAlwaysShowColliders(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater and editor
		/// </summary>
		public static bool HasShowColliderAABB(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool HasContactArrowScale(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater and editor
		/// </summary>
		public static bool HasColliderAABBColorRGBAf(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5, 5);

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

			if (HasSimulationMode(reader.Version))
			{
				SimulationMode = reader.ReadInt32();
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
			if (HasReuseCollisionCallbacks(reader.Version))
			{
#warning This might be present prior to 2019.4. Check. 
				ReuseCollisionCallbacks = reader.ReadBoolean();
			}
			if (HasAutoSyncTransforms(reader.Version))
			{
				AutoSyncTransforms = reader.ReadBoolean();
			}

			reader.AlignStream();

			LayerCollisionMatrix = reader.ReadUInt32Array();
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
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
			node.Add(ColliderAwakeColorName, GetColliderAwakeColorRGBAf(container.Version, container.Flags).ExportYAML(container));
			node.Add(ColliderAsleepColorName, GetColliderAsleepColorRGBAf(container.Version, container.Flags).ExportYAML(container));
			node.Add(ColliderContactColorName, GetColliderContactColorRGBAf(container.Version, container.Flags).ExportYAML(container));
			node.Add(ColliderAABBColorName, GetColliderAABBColorRGBAf(container.Version, container.Flags).ExportYAML(container));
			node.Add(LayerCollisionMatrixName, LayerCollisionMatrix.ExportYAML(true));
			return node;
		}

		private float GetVelocityThreshold(UnityVersion version)
		{
			return HasVelocityThreshold(version) ? VelocityThreshold : 1.0f;
		}
		private float GetMaxLinearCorrection(UnityVersion version)
		{
			return HasVelocityThreshold(version) ? MaxLinearCorrection : 0.2f;
		}
		private float GetMaxAngularCorrection(UnityVersion version)
		{
			return HasVelocityThreshold(version) ? MaxAngularCorrection : 8.0f;
		}
		private float GetMaxTranslationSpeed(UnityVersion version)
		{
			return HasVelocityThreshold(version) ? MaxTranslationSpeed : 100.0f;
		}
		private float GetMaxRotationSpeed(UnityVersion version)
		{
			return HasVelocityThreshold(version) ? MaxRotationSpeed : 360.0f;
		}
		private float GetBaumgarteScale(UnityVersion version)
		{
			return HasBaumgarteScale(version) ? BaumgarteScale : 0.2f;
		}
		private float GetBaumgarteTimeOfImpactScale(UnityVersion version)
		{
			return HasBaumgarteScale(version) ? BaumgarteTimeOfImpactScale : 0.75f;
		}
		private float GetTimeToSleep(UnityVersion version)
		{
			return HasBaumgarteScale(version) ? TimeToSleep : 0.5f;
		}
		private float GetLinearSleepTolerance(UnityVersion version)
		{
			return HasBaumgarteScale(version) ? LinearSleepTolerance : 0.01f;
		}
		private float GetAngularSleepTolerance(UnityVersion version)
		{
			return HasBaumgarteScale(version) ? AngularSleepTolerance : 2.0f;
		}
		private float GetDefaultContactOffset(UnityVersion version)
		{
			return HasDefaultContactOffset(version) ? DefaultContactOffset : 0.01f;
		}
		private bool GetAutoSimulation(UnityVersion version)
		{
			return HasAutoSimulation(version) ? AutoSimulation : true;
		}
		private bool GetQueriesStartInColliders(UnityVersion version)
		{
			return HasQueriesStartInColliders(version) ? QueriesStartInColliders : true;
		}
		private bool GetCallbacksOnDisable(UnityVersion version)
		{
			return HasCallbacksOnDisable(version) ? CallbacksOnDisable : true;
		}
		private bool GetAutoSyncTransforms(UnityVersion version)
		{
			return HasAutoSyncTransforms(version) ? AutoSyncTransforms : true;
		}
		private bool GetAlwaysShowColliders()
		{
			return false;
		}
		private bool GetShowColliderSleep(UnityVersion version, TransferInstructionFlags flags)
		{
			return true;
		}
		private bool GetShowColliderContacts()
		{
			return false;
		}
		private bool GetShowColliderAABB()
		{
			return false;
		}
		private float GetContactArrowScale(UnityVersion version, TransferInstructionFlags flags)
		{
			return 0.2f;
		}
		private ColorRGBAf GetColliderAwakeColorRGBAf(UnityVersion version, TransferInstructionFlags flags)
		{
			return new ColorRGBAf(0.5686275f, 0.95686275f, 0.54509807f, 0.7529412f);
		}
		private ColorRGBAf GetColliderAsleepColorRGBAf(UnityVersion version, TransferInstructionFlags flags)
		{
			return new ColorRGBAf(0.5686275f, 0.95686275f, 0.54509807f, 0.36078432f);
		}
		private ColorRGBAf GetColliderContactColorRGBAf(UnityVersion version, TransferInstructionFlags flags)
		{
			return new ColorRGBAf(1.0f, 0.0f, 1.0f, 0.6862745f);
		}
		private ColorRGBAf GetColliderAABBColorRGBAf(UnityVersion version, TransferInstructionFlags flags)
		{
			return new ColorRGBAf(1.0f, 1.0f, 0.0f, 0.2509804f);
		}

		public int VelocityIterations { get; set; }
		public int PositionIterations { get; set; }
		public int SimulationMode { get; set; }
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

		public bool ReuseCollisionCallbacks { get; set; }
		public bool AutoSyncTransforms { get; set; }

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

		public Vector2f Gravity = new();
		public PPtr<PhysicsMaterial2D> DefaultMaterial = new();
		public PhysicsJobOptions2D JobOptions = new();
	}
}
