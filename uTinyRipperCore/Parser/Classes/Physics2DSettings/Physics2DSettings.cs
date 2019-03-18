using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Physics2DSettingss;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

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
			m_layerCollisionMatrix = new uint[32];
			for(int i = 0; i < m_layerCollisionMatrix.Length; i++)
			{
				m_layerCollisionMatrix[i] = uint.MaxValue;
			}
		}

		public static Physics2DSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new Physics2DSettings(assetInfo, true));
		}

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadPhysics2DSettings(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadVelocityThreshold(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 4.6.1 to 5.6.0 exclusive
		/// </summary>
		public static bool IsReadMinPenetrationForPenalty(Version version)
		{
			return version.IsLess(5, 6) && version.IsGreaterEqual(4, 6, 1);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadBaumgarteScale(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadDefaultContactOffset(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadJobOptions(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool IsReadAutoSimulation(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 4.6.1 and greater
		/// </summary>
		public static bool IsReadQueriesStartInColliders(Version version)
		{
			return version.IsGreaterEqual(4, 6, 1);
		}
		/// <summary>
		/// 4.5.3 to 4.6.0
		/// </summary>
		public static bool IsReadDeleteStopsCallbacks(Version version)
		{
			return version.IsLessEqual(4, 6) && version.IsGreaterEqual(4, 5, 3);
		}
		/// <summary>
		/// 4.6.1 to 2018.1 exclusive
		/// </summary>
		public static bool IsReadChangeStopsCallbacks(Version version)
		{
			return version.IsGreaterEqual(4, 6, 1) && version.IsLess(2018);
		}
		/// <summary>
		/// 5.6.1 and greater
		/// </summary>
		public static bool IsReadCallbacksOnDisable(Version version)
		{
			return version.IsGreaterEqual(5, 6, 1);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadAutoSyncTransforms(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool IsReadAlwaysShowColliders(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.5.0 and greater and editor
		/// </summary>
		public static bool IsReadShowColliderAABB(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool IsReadContactArrowScale(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.5.0 and greater and editor
		/// </summary>
		public static bool IsReadColliderAABBColor(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5, 5);
		}

		private static int GetSerializedVersion(Version version)
		{
			// disabled RaycastsHitTriggers backward compatibility?
			if (Config.IsExportTopmostSerializedVersion || version.IsGreaterEqual(5, 6))
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Gravity.Read(reader);
			DefaultMaterial.Read(reader);
			VelocityIterations = reader.ReadInt32();
			PositionIterations = reader.ReadInt32();
			if(IsReadVelocityThreshold(reader.Version))
			{
				VelocityThreshold = reader.ReadSingle();
				MaxLinearCorrection = reader.ReadSingle();
				MaxAngularCorrection = reader.ReadSingle();
				MaxTranslationSpeed = reader.ReadSingle();
				MaxRotationSpeed = reader.ReadSingle();
			}
			if (IsReadMinPenetrationForPenalty(reader.Version))
			{
				MinPenetrationForPenalty = reader.ReadSingle();
			}
			if (IsReadBaumgarteScale(reader.Version))
			{
				BaumgarteScale = reader.ReadSingle();
				BaumgarteTimeOfImpactScale = reader.ReadSingle();
				TimeToSleep = reader.ReadSingle();
				LinearSleepTolerance = reader.ReadSingle();
				AngularSleepTolerance = reader.ReadSingle();
			}
			if (IsReadDefaultContactOffset(reader.Version))
			{
				DefaultContactOffset = reader.ReadSingle();
			}
			if (IsReadJobOptions(reader.Version))
			{
				JobOptions.Read(reader);
			}
			if (IsReadAutoSimulation(reader.Version))
			{
				AutoSimulation = reader.ReadBoolean();
			}
			QueriesHitTriggers = reader.ReadBoolean();
			if (IsReadQueriesStartInColliders(reader.Version))
			{
				QueriesStartInColliders = reader.ReadBoolean();
			}
			if (IsReadDeleteStopsCallbacks(reader.Version))
			{
				DeleteStopsCallbacks = reader.ReadBoolean();
			}
			if (IsReadChangeStopsCallbacks(reader.Version))
			{
				ChangeStopsCallbacks = reader.ReadBoolean();
			}
			if (IsReadCallbacksOnDisable(reader.Version))
			{
				CallbacksOnDisable = reader.ReadBoolean();
			}
			if (IsReadAutoSyncTransforms(reader.Version))
			{
				AutoSyncTransforms = reader.ReadBoolean();
			}
#if UNIVERSAL
			if (IsReadAlwaysShowColliders(reader.Version, reader.Flags))
			{
				AlwaysShowColliders = reader.ReadBoolean();
				ShowColliderSleep = reader.ReadBoolean();
				ShowColliderContacts = reader.ReadBoolean();
			}
			if (IsReadShowColliderAABB(reader.Version, reader.Flags))
			{
				ShowColliderAABB = reader.ReadBoolean();
			}
#endif
			reader.AlignStream(AlignType.Align4);

#if UNIVERSAL
			if (IsReadContactArrowScale(reader.Version, reader.Flags))
			{
				ContactArrowScale = reader.ReadSingle();
				ColliderAwakeColor.Read(reader);
				ColliderAsleepColor.Read(reader);
				ColliderContactColor.Read(reader);
			}
			if (IsReadColliderAABBColor(reader.Version, reader.Flags))
			{
				ColliderAABBColor.Read(reader);
			}
#endif
			m_layerCollisionMatrix = reader.ReadUInt32Array();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return DefaultMaterial.FetchDependency(file, isLog, ToLogString, "m_DefaultMaterial");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Gravity", Gravity.ExportYAML(container));
			node.Add("m_DefaultMaterial", DefaultMaterial.ExportYAML(container));
			node.Add("m_VelocityIterations", VelocityIterations);
			node.Add("m_PositionIterations", PositionIterations);
			node.Add("m_VelocityThreshold", GetVelocityThreshold(container.Version));
			node.Add("m_MaxLinearCorrection", GetMaxLinearCorrection(container.Version));
			node.Add("m_MaxAngularCorrection", GetMaxAngularCorrection(container.Version));
			node.Add("m_MaxTranslationSpeed", GetMaxTranslationSpeed(container.Version));
			node.Add("m_MaxRotationSpeed", GetMaxRotationSpeed(container.Version));
			node.Add("m_BaumgarteScale", GetBaumgarteScale(container.Version));
			node.Add("m_BaumgarteTimeOfImpactScale", GetBaumgarteTimeOfImpactScale(container.Version));
			node.Add("m_TimeToSleep", GetTimeToSleep(container.Version));
			node.Add("m_LinearSleepTolerance", GetLinearSleepTolerance(container.Version));
			node.Add("m_AngularSleepTolerance", GetAngularSleepTolerance(container.Version));
			node.Add("m_DefaultContactOffset", GetDefaultContactOffset(container.Version));
			// 2018
			//node.Add("m_JobOptions", GetJobOptions(container.Version));
			node.Add("m_AutoSimulation", GetAutoSimulation(container.Version));
			node.Add("m_QueriesHitTriggers", QueriesHitTriggers);
			node.Add("m_QueriesStartInColliders", GetQueriesStartInColliders(container.Version));
			node.Add("m_ChangeStopsCallbacks", ChangeStopsCallbacks);
			node.Add("m_CallbacksOnDisable", GetCallbacksOnDisable(container.Version));
			node.Add("m_AutoSyncTransforms", GetAutoSyncTransforms(container.Version));
			node.Add("m_AlwaysShowColliders", GetAlwaysShowColliders());
			node.Add("m_ShowColliderSleep", GetShowColliderSleep(container.Version, container.Flags));
			node.Add("m_ShowColliderContacts", GetShowColliderContacts());
			node.Add("m_ShowColliderAABB", GetShowColliderAABB());
			node.Add("m_ContactArrowScale", GetContactArrowScale(container.Version, container.Flags));
			node.Add("m_ColliderAwakeColor", GetColliderAwakeColor(container.Version, container.Flags).ExportYAML(container));
			node.Add("m_ColliderAsleepColor", GetColliderAsleepColor(container.Version, container.Flags).ExportYAML(container));
			node.Add("m_ColliderContactColor", GetColliderContactColor(container.Version, container.Flags).ExportYAML(container));
			node.Add("m_ColliderAABBColor", GetColliderAABBColor(container.Version, container.Flags).ExportYAML(container));
			node.Add("m_LayerCollisionMatrix", LayerCollisionMatrix.ExportYAML(true));
			return node;
		}

		private float GetVelocityThreshold(Version version)
		{
			return IsReadVelocityThreshold(version) ? VelocityThreshold : 1.0f;
		}
		private float GetMaxLinearCorrection(Version version)
		{
			return IsReadVelocityThreshold(version) ? MaxLinearCorrection : 0.2f;
		}
		private float GetMaxAngularCorrection(Version version)
		{
			return IsReadVelocityThreshold(version) ? MaxAngularCorrection : 8.0f;
		}
		private float GetMaxTranslationSpeed(Version version)
		{
			return IsReadVelocityThreshold(version) ? MaxTranslationSpeed : 100.0f;
		}
		private float GetMaxRotationSpeed(Version version)
		{
			return IsReadVelocityThreshold(version) ? MaxRotationSpeed : 360.0f;
		}
		private float GetBaumgarteScale(Version version)
		{
			return IsReadBaumgarteScale(version) ? BaumgarteScale : 0.2f;
		}
		private float GetBaumgarteTimeOfImpactScale(Version version)
		{
			return IsReadBaumgarteScale(version) ? BaumgarteTimeOfImpactScale : 0.75f;
		}
		private float GetTimeToSleep(Version version)
		{
			return IsReadBaumgarteScale(version) ? TimeToSleep : 0.5f;
		}
		private float GetLinearSleepTolerance(Version version)
		{
			return IsReadBaumgarteScale(version) ? LinearSleepTolerance : 0.01f;
		}
		private float GetAngularSleepTolerance(Version version)
		{
			return IsReadBaumgarteScale(version) ? AngularSleepTolerance : 2.0f;
		}
		private float GetDefaultContactOffset(Version version)
		{
			return IsReadDefaultContactOffset(version) ? DefaultContactOffset : 0.01f;
		}
		private bool GetAutoSimulation(Version version)
		{
			return IsReadAutoSimulation(version) ? AutoSimulation : true;
		}
		private bool GetQueriesStartInColliders(Version version)
		{
			return IsReadQueriesStartInColliders(version) ? QueriesStartInColliders : true;
		}
		private bool GetCallbacksOnDisable(Version version)
		{
			return IsReadCallbacksOnDisable(version) ? CallbacksOnDisable : true;
		}
		private bool GetAutoSyncTransforms(Version version)
		{
			return IsReadAutoSyncTransforms(version) ? AutoSyncTransforms : true;
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
			if(IsReadAlwaysShowColliders(version, flags))
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
			if(IsReadContactArrowScale(version, flags))
			{
				return ContactArrowScale;
			}
#endif
			return 0.2f;
		}
		private ColorRGBAf GetColliderAwakeColor(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadContactArrowScale(version, flags))
			{
				return ColliderAwakeColor;
			}
#endif
			return new ColorRGBAf(0.5686275f, 0.95686275f, 0.54509807f, 0.7529412f);
		}
		private ColorRGBAf GetColliderAsleepColor(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadContactArrowScale(version, flags))
			{
				return ColliderAsleepColor;
			}
#endif
			return new ColorRGBAf(0.5686275f, 0.95686275f, 0.54509807f, 0.36078432f);
		}
		private ColorRGBAf GetColliderContactColor(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadContactArrowScale(version, flags))
			{
				return ColliderContactColor;
			}
#endif
			return new ColorRGBAf(1.0f, 0.0f, 1.0f, 0.6862745f);
		}
		private ColorRGBAf GetColliderAABBColor(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if(IsReadContactArrowScale(version, flags))
			{
				return ColliderAABBColor;
			}
#endif
			return new ColorRGBAf(1.0f, 1.0f, 0.0f, 0.2509804f);
		}

		public int VelocityIterations { get; private set; }
		public int PositionIterations { get; private set; }
		public float VelocityThreshold { get; private set; }
		public float MaxLinearCorrection { get; private set; }
		public float MaxAngularCorrection { get; private set; }
		public float MaxTranslationSpeed { get; private set; }
		public float MaxRotationSpeed { get; private set; }
		public float MinPenetrationForPenalty { get; private set; }
		public float BaumgarteScale { get; private set; }
		public float BaumgarteTimeOfImpactScale { get; private set; }
		public float TimeToSleep { get; private set; }
		public float LinearSleepTolerance { get; private set; }
		public float AngularSleepTolerance { get; private set; }
		public float DefaultContactOffset { get; private set; }
		public bool AutoSimulation { get; private set; }
		/// <summary>
		/// RaycastsHitTriggers previously
		/// </summary>
		public bool QueriesHitTriggers { get; private set; }
		/// <summary>
		/// RaycastsStartInColliders previously
		/// </summary>
		public bool QueriesStartInColliders { get; private set; }
		public bool DeleteStopsCallbacks { get; private set; }
		public bool ChangeStopsCallbacks { get; private set; }
		public bool CallbacksOnDisable { get; private set; }
		public bool AutoSyncTransforms { get; private set; }
#if UNIVERSAL
		public bool AlwaysShowColliders { get; private set; }
		public bool ShowColliderSleep { get; private set; }
		public bool ShowColliderContacts { get; private set; }
		public bool ShowColliderAABB { get; private set; }
		public float ContactArrowScale { get; private set; }
#endif
		public IReadOnlyList<uint> LayerCollisionMatrix => m_layerCollisionMatrix;

		public Vector2f Gravity;
		public PPtr<PhysicsMaterial2D> DefaultMaterial;
		public PhysicsJobOptions2D JobOptions;
#if UNIVERSAL
		public ColorRGBAf ColliderAwakeColor;
		public ColorRGBAf ColliderAsleepColor;
		public ColorRGBAf ColliderContactColor;
		public ColorRGBAf ColliderAABBColor;
#endif

		private uint[] m_layerCollisionMatrix;
	}
}
