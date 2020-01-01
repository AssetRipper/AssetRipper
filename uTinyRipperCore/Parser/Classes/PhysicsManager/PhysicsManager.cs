using System.Collections.Generic;
using uTinyRipper.Classes.PhysicsManagers;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class PhysicsManager : GlobalGameManager
	{
		public PhysicsManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// DefaultMaxAngluarSpeed has been renamed to DefaultMaxAngularSpeed
			if (version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 5))
			{
				return 13;
			}
			// unknown
			if (version.IsGreaterEqual(2019))
			{
				return 12;
			}
			// Default value for DefaultMaxAngularSpeed has been changed from 7.0f to 50.0f
			//return 11;

			// unknown changes
			if (version.IsGreaterEqual(2018, 3))
			{
				return 10;
			}

			// somewhere in 2018.3 alpha/beta
			//return 9;
			//return 8;

			if (version.IsGreaterEqual(2017, 3))
			{
				return 7;
			}

			// somewhere in 2017.3 alpha
			// return 6;
			// return 5;
			// EnablePCM converted to ContactsGeneration
			// return 4;

			// SolverIterationCount renamed to DefaultSolverIterations
			// SolverVelocityIterations renamed to DefaultSolverVelocityIterations
			if (version.IsGreaterEqual(5, 5))
			{
				return 3;
			}
			// RaycastsHitTriggers renamed to QueriesHitTriggers
			if (version.IsGreaterEqual(5, 2, 1))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasSleepThreshold(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0b1 and less
		/// </summary>
		public static bool HasSleepAngularVelocity(Version version) => version.IsLessEqual(5, 0, 0, VersionType.Beta, 1);
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool HasDefaultContactOffset(Version version) => version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasDefaultSolverVelocityIterations(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasQueriesHitBackfaces(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasQueriesHitTriggers(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasEnableAdaptiveForce(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0 to 2017.2
		/// </summary>
		public static bool HasEnablePCM(Version version) => version.IsGreaterEqual(5) && version.IsLessEqual(2017, 2);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasClothInterCollisionDistance(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasLayerCollisionMatrix(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasAutoSimulation(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasAutoSyncTransforms(Version version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasClothInterCollisionSettingsToggle(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasClothGravity(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasContactPairsMode(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasFrictionType(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasSolverType(Version version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasDefaultMaxAngularSpeed(Version version) => version.IsGreaterEqual(2019);

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsAlign2(Version version) => version.IsGreaterEqual(2019);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Gravity.Read(reader);
			DefaultMaterial.Read(reader);
			BounceThreshold = reader.ReadSingle();
			if (HasSleepThreshold(reader.Version))
			{
				SleepThreshold = reader.ReadSingle();
			}
			else
			{
				SleepVelocity = reader.ReadSingle();
				SleepAngularVelocity = reader.ReadSingle();
			}
			if (HasSleepAngularVelocity(reader.Version))
			{
				MaxAngularVelocity = reader.ReadSingle();
			}
			if (HasDefaultContactOffset(reader.Version))
			{
				DefaultContactOffset = reader.ReadSingle();
			}
			else
			{
				MinPenetrationForPenalty = reader.ReadSingle();
			}
			DefaultSolverIterations = reader.ReadInt32();
			if (HasDefaultSolverVelocityIterations(reader.Version))
			{
				DefaultSolverVelocityIterations = reader.ReadInt32();
			}
			if (HasQueriesHitBackfaces(reader.Version))
			{
				QueriesHitBackfaces = reader.ReadBoolean();
			}
			if (HasQueriesHitTriggers(reader.Version))
			{
				QueriesHitTriggers = reader.ReadBoolean();
			}
			if (HasEnableAdaptiveForce(reader.Version))
			{
				EnableAdaptiveForce = reader.ReadBoolean();
			}
			if (HasEnablePCM(reader.Version))
			{
				EnablePCM = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasClothInterCollisionDistance(reader.Version))
			{
				ClothInterCollisionDistance = reader.ReadSingle();
				ClothInterCollisionStiffness = reader.ReadSingle();
				ContactsGeneration = (ContactsGeneration)reader.ReadInt32();
				reader.AlignStream();
			}

			if (HasLayerCollisionMatrix(reader.Version))
			{
				LayerCollisionMatrix = reader.ReadUInt32Array();
			}

			if (HasAutoSimulation(reader.Version))
			{
				AutoSimulation = reader.ReadBoolean();
			}
			if (HasAutoSyncTransforms(reader.Version))
			{
				AutoSyncTransforms = reader.ReadBoolean();
			}
			if (HasClothInterCollisionSettingsToggle(reader.Version))
			{
				ClothInterCollisionSettingsToggle = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasClothGravity(reader.Version))
			{
				ClothGravity.Read(reader);
			}
			if (HasContactPairsMode(reader.Version))
			{
				ContactPairsMode = (ContactPairsMode)reader.ReadInt32();
				BroadphaseType = (BroadphaseType)reader.ReadInt32();
				WorldBounds.Read(reader);
				WorldSubdivisions = reader.ReadInt32();
			}
			if (HasFrictionType(reader.Version))
			{
				FrictionType = (FrictionType)reader.ReadInt32();
				EnableEnhancedDeterminism = reader.ReadBoolean();
				EnableUnifiedHeightmaps = reader.ReadBoolean();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasSolverType(reader.Version))
			{
				SolverType = (SolverType)reader.ReadInt32();
			}
			if (HasDefaultMaxAngularSpeed(reader.Version))
			{
				DefaultMaxAngularSpeed = reader.ReadSingle();
			}
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
			node.Add(BounceThresholdName, BounceThreshold);
			node.Add(SleepThresholdName, GetSleepThreshold(container.Version));
			node.Add(DefaultContactOffsetName, GetDefaultContactOffset(container.Version));
			node.Add(DefaultSolverIterationsName, DefaultSolverIterations);
			node.Add(DefaultSolverVelocityIterationsName, GetDefaultSolverVelocityIterations(container.Version));
			node.Add(QueriesHitBackfacesName, QueriesHitBackfaces);
			node.Add(QueriesHitTriggersName, GetQueriesHitTriggers(container.Version));
			node.Add(EnableAdaptiveForceName, EnableAdaptiveForce);
			node.Add(ClothInterCollisionDistanceName, ClothInterCollisionDistance);
			node.Add(ClothInterCollisionStiffnessName, ClothInterCollisionStiffness);
			node.Add(ContactsGenerationName, (int)GetContactsGeneration(container.Version));
			node.Add(LayerCollisionMatrixName, GetLayerCollisionMatrix(container.Version).ExportYAML(true));
			node.Add(AutoSimulationName, GetAutoSimulation(container.Version));
			node.Add(AutoSyncTransformsName, GetAutoSyncTransforms(container.Version));
			node.Add(ReuseCollisionCallbacksName, ReuseCollisionCallbacks);
			node.Add(ClothInterCollisionSettingsToggleName, ClothInterCollisionSettingsToggle);
			if (HasClothGravity(container.ExportVersion))
			{
				node.Add(ClothGravityName, GetClothGravity(container.Version).ExportYAML(container));
			}
			node.Add(ContactPairsModeName, (int)ContactPairsMode);
			node.Add(BroadphaseTypeName, (int)BroadphaseType);
			node.Add(WorldBoundsName, GetWorldBounds(container.Version).ExportYAML(container));
			node.Add(WorldSubdivisionsName, GetWorldSubdivisions(container.Version));
			if (HasFrictionType(container.ExportVersion))
			{
				node.Add(FrictionTypeName, (int)FrictionType);
				node.Add(EnableEnhancedDeterminismName, EnableEnhancedDeterminism);
				node.Add(EnableUnifiedHeightmapsName, GetEnableUnifiedHeightmaps(container.Version));
			}
			if (HasSolverType(container.ExportVersion))
			{
				node.Add(SolverTypeName, (int)SolverType);
			}
			if (HasDefaultMaxAngularSpeed(container.ExportVersion))
			{
				node.Add(ToSerializedVersion(container.ExportVersion) < 13 ? DefaultMaxAngluarSpeedName : DefaultMaxAngularSpeedName,
					GetDefaultMaxAngularSpeed(container.Version));
			}
			return node;
		}

		private float GetSleepThreshold(Version version)
		{
			return HasSleepThreshold(version) ? SleepThreshold : 0.005f;
		}
		private float GetDefaultContactOffset(Version version)
		{
			return HasDefaultContactOffset(version) ? DefaultContactOffset : 0.01f;
		}
		private int GetDefaultSolverVelocityIterations(Version version)
		{
			return HasDefaultSolverVelocityIterations(version) ? DefaultSolverVelocityIterations : 1;
		}
		private bool GetQueriesHitTriggers(Version version)
		{
			return HasQueriesHitTriggers(version) ? QueriesHitTriggers : true;
		}
		private ContactsGeneration GetContactsGeneration(Version version)
		{
			if (HasClothInterCollisionDistance(version))
			{
				return ContactsGeneration;
			}
			return EnablePCM ? ContactsGeneration.PersistentContactManifold : ContactsGeneration.LegacyContactsGeneration;
		}
		private IReadOnlyList<uint> GetLayerCollisionMatrix(Version version)
		{
			if (HasLayerCollisionMatrix(version))
			{
				return LayerCollisionMatrix;
			}
			uint[] matrix = new uint[32];
			for(int i = 0; i < matrix.Length; i++)
			{
				matrix[i] = uint.MaxValue;
			}
			return matrix;
		}
		private bool GetAutoSimulation(Version version)
		{
			return HasAutoSimulation(version) ? AutoSimulation : true;
		}
		private bool GetAutoSyncTransforms(Version version)
		{
			return HasAutoSyncTransforms(version) ? AutoSyncTransforms : true;
		}
		private Vector3f GetClothGravity(Version version)
		{
			return HasClothGravity(version) ? ClothGravity : new Vector3f(0.0f, -9.81f, 0.0f);
		}
		private AABB GetWorldBounds(Version version)
		{
			if (HasAutoSyncTransforms(version))
			{
				return WorldBounds;
			}
			return new AABB(default, new Vector3f(250.0f, 250.0f, 250.0f));
		}
		private int GetWorldSubdivisions(Version version)
		{
			return HasClothInterCollisionSettingsToggle(version) ? WorldSubdivisions : 8;
		}
		private bool GetEnableUnifiedHeightmaps(Version version)
		{
			return HasFrictionType(version) ? EnableUnifiedHeightmaps : true;
		}
		private float GetDefaultMaxAngularSpeed(Version version)
		{
			return HasDefaultMaxAngularSpeed(version) ? DefaultMaxAngularSpeed : 7.0f;
		}

		public float BounceThreshold { get; set; }
		public float SleepThreshold { get; set; }
		public float SleepVelocity { get; set; }
		public float SleepAngularVelocity { get; set; }
		public float MaxAngularVelocity { get; set; }
		public float MinPenetrationForPenalty { get; set; }
		public float DefaultContactOffset { get; set; }
		/// <summary>
		/// SolverIterationCount previosuly
		/// </summary>
		public int DefaultSolverIterations { get; set; }
		/// <summary>
		/// SolverVelocityIterations previously
		/// </summary>
		public int DefaultSolverVelocityIterations { get; set; }
		public bool QueriesHitBackfaces { get; set; }
		/// <summary>
		/// RaycastsHitTriggers previosly
		/// </summary>
		public bool QueriesHitTriggers { get; set; }
		public bool EnableAdaptiveForce { get; set; }
		public bool EnablePCM { get; set; }
		public float ClothInterCollisionDistance { get; set; }
		public float ClothInterCollisionStiffness { get; set; }
		public ContactsGeneration ContactsGeneration { get; set; }
		public uint[] LayerCollisionMatrix { get; set; }
		public bool AutoSimulation { get; set; }
		public bool AutoSyncTransforms { get; set; }
		public bool ReuseCollisionCallbacks { get; set; }
		public bool ClothInterCollisionSettingsToggle { get; set; }
		public ContactPairsMode ContactPairsMode { get; set; }
		public BroadphaseType BroadphaseType { get; set; }
		public int WorldSubdivisions { get; set; }
		public FrictionType FrictionType { get; set; }
		public bool EnableEnhancedDeterminism { get; set; }
		public bool EnableUnifiedHeightmaps { get; set; }
		public SolverType SolverType { get; set; }
		/// <summary>
		/// DefaultMaxAngluarSpeed previously
		/// </summary>
		public float DefaultMaxAngularSpeed { get; set; }

		public const string GravityName = "m_Gravity";
		public const string DefaultMaterialName = "m_DefaultMaterial";
		public const string BounceThresholdName = "m_BounceThreshold";
		public const string SleepThresholdName = "m_SleepThreshold";
		public const string DefaultContactOffsetName = "m_DefaultContactOffset";
		public const string DefaultSolverIterationsName = "m_DefaultSolverIterations";
		public const string DefaultSolverVelocityIterationsName = "m_DefaultSolverVelocityIterations";
		public const string QueriesHitBackfacesName = "m_QueriesHitBackfaces";
		public const string QueriesHitTriggersName = "m_QueriesHitTriggers";
		public const string EnableAdaptiveForceName = "m_EnableAdaptiveForce";
		public const string ClothInterCollisionDistanceName = "m_ClothInterCollisionDistance";
		public const string ClothInterCollisionStiffnessName = "m_ClothInterCollisionStiffness";
		public const string ContactsGenerationName = "m_ContactsGeneration";
		public const string LayerCollisionMatrixName = "m_LayerCollisionMatrix";
		public const string AutoSimulationName = "m_AutoSimulation";
		public const string AutoSyncTransformsName = "m_AutoSyncTransforms";
		public const string ReuseCollisionCallbacksName = "m_ReuseCollisionCallbacks";
		public const string ClothInterCollisionSettingsToggleName = "m_ClothInterCollisionSettingsToggle";
		public const string ClothGravityName = "m_ClothGravity";
		public const string ContactPairsModeName = "m_ContactPairsMode";
		public const string BroadphaseTypeName = "m_BroadphaseType";
		public const string WorldBoundsName = "m_WorldBounds";
		public const string WorldSubdivisionsName = "m_WorldSubdivisions";
		public const string FrictionTypeName = "m_FrictionType";
		public const string EnableEnhancedDeterminismName = "m_EnableEnhancedDeterminism";
		public const string EnableUnifiedHeightmapsName = "m_EnableUnifiedHeightmaps";
		public const string SolverTypeName = "m_SolverType";
		public const string DefaultMaxAngluarSpeedName = "m_DefaultMaxAngluarSpeed";
		public const string DefaultMaxAngularSpeedName = "m_DefaultMaxAngularSpeed";

		public Vector3f Gravity;
		public PPtr<PhysicMaterial> DefaultMaterial;
		public Vector3f ClothGravity;
		public AABB WorldBounds;
	}
}
