using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.PhysicsManagers;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class PhysicsManager : GlobalGameManager
	{
		public PhysicsManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadSleepThreshold(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0b1 and less
		/// </summary>
		public static bool IsReadSleepAngularVelocity(Version version)
		{
			return version.IsLessEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadDefaultContactOffset(Version version)
		{
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadDefaultSolverVelocityIterations(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadQueriesHitBackfaces(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadQueriesHitTriggers(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadEnableAdaptiveForce(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0 to 2017.2
		/// </summary>
		public static bool IsReadEnablePCM(Version version)
		{
			return version.IsGreaterEqual(5) && version.IsLessEqual(2017, 2);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadClothInterCollisionDistance(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadLayerCollisionMatrix(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool IsReadAutoSimulation(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadAutoSyncTransforms(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadClothInterCollisionSettingsToggle(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadClothGravity(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadContactPairsMode(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadFrictionType(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadDefaultMaxAngularSpeed(Version version)
		{
			return version.IsGreaterEqual(2019);
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(3);
		}

		private static int GetSerializedVersion(Version version)
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Gravity.Read(reader);
			DefaultMaterial.Read(reader);
			BounceThreshold = reader.ReadSingle();
			if (IsReadSleepThreshold(reader.Version))
			{
				SleepThreshold = reader.ReadSingle();
			}
			else
			{
				SleepVelocity = reader.ReadSingle();
				SleepAngularVelocity = reader.ReadSingle();
			}
			if (IsReadSleepAngularVelocity(reader.Version))
			{
				MaxAngularVelocity = reader.ReadSingle();
			}
			if(IsReadDefaultContactOffset(reader.Version))
			{
				DefaultContactOffset = reader.ReadSingle();
			}
			else
			{
				MinPenetrationForPenalty = reader.ReadSingle();
			}
			DefaultSolverIterations = reader.ReadInt32();
			if (IsReadDefaultSolverVelocityIterations(reader.Version))
			{
				DefaultSolverVelocityIterations = reader.ReadInt32();
			}
			if (IsReadQueriesHitBackfaces(reader.Version))
			{
				QueriesHitBackfaces = reader.ReadBoolean();
			}
			if (IsReadQueriesHitTriggers(reader.Version))
			{
				QueriesHitTriggers = reader.ReadBoolean();
			}
			if (IsReadEnableAdaptiveForce(reader.Version))
			{
				EnableAdaptiveForce = reader.ReadBoolean();
			}
			if (IsReadEnablePCM(reader.Version))
			{
				EnablePCM = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadClothInterCollisionDistance(reader.Version))
			{
				ClothInterCollisionDistance = reader.ReadSingle();
				ClothInterCollisionStiffness = reader.ReadSingle();
				ContactsGeneration = (ContactsGeneration)reader.ReadInt32();
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadLayerCollisionMatrix(reader.Version))
			{
				m_layerCollisionMatrix = reader.ReadUInt32Array();
			}

			if (IsReadAutoSimulation(reader.Version))
			{
				AutoSimulation = reader.ReadBoolean();
			}
			if (IsReadAutoSyncTransforms(reader.Version))
			{
				AutoSyncTransforms = reader.ReadBoolean();
			}
			if (IsReadClothInterCollisionSettingsToggle(reader.Version))
			{
				ClothInterCollisionSettingsToggle = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadClothGravity(reader.Version))
			{
				ClothGravity.Read(reader);
			}
			if (IsReadContactPairsMode(reader.Version))
			{
				ContactPairsMode = (ContactPairsMode)reader.ReadInt32();
				BroadphaseType = (BroadphaseType)reader.ReadInt32();
				WorldBounds.Read(reader);
				WorldSubdivisions = reader.ReadInt32();
			}
			if (IsReadFrictionType(reader.Version))
			{
				FrictionType = (FrictionType)reader.ReadInt32();
				EnableEnhancedDeterminism = reader.ReadBoolean();
				EnableUnifiedHeightmaps = reader.ReadBoolean();
			}
			if (IsReadDefaultMaxAngularSpeed(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);

				DefaultMaxAngularSpeed = reader.ReadSingle();
			}
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
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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
			if (IsReadClothGravity(container.ExportVersion))
			{
				node.Add(ClothGravityName, GetClothGravity(container.Version).ExportYAML(container));
			}
			node.Add(ContactPairsModeName, (int)ContactPairsMode);
			node.Add(BroadphaseTypeName, (int)BroadphaseType);
			node.Add(WorldBoundsName, GetWorldBounds(container.Version).ExportYAML(container));
			node.Add(WorldSubdivisionsName, GetWorldSubdivisions(container.Version));
			if (IsReadFrictionType(container.ExportVersion))
			{
				node.Add(FrictionTypeName, (int)FrictionType);
				node.Add(EnableEnhancedDeterminismName, EnableEnhancedDeterminism);
				node.Add(EnableUnifiedHeightmapsName, GetEnableUnifiedHeightmaps(container.Version));
			}
			if (IsReadDefaultMaxAngularSpeed(container.ExportVersion))
			{
				node.Add(GetSerializedVersion(container.ExportVersion) < 13 ? DefaultMaxAngluarSpeedName : DefaultMaxAngularSpeedName,
					GetDefaultMaxAngularSpeed(container.Version));
			}
			return node;
		}

		private float GetSleepThreshold(Version version)
		{
			return IsReadSleepThreshold(version) ? SleepThreshold : 0.005f;
		}
		private float GetDefaultContactOffset(Version version)
		{
			return IsReadDefaultContactOffset(version) ? DefaultContactOffset : 0.01f;
		}
		private int GetDefaultSolverVelocityIterations(Version version)
		{
			return IsReadDefaultSolverVelocityIterations(version) ? DefaultSolverVelocityIterations : 1;
		}
		private bool GetQueriesHitTriggers(Version version)
		{
			return IsReadQueriesHitTriggers(version) ? QueriesHitTriggers : true;
		}
		private ContactsGeneration GetContactsGeneration(Version version)
		{
			if (IsReadClothInterCollisionDistance(version))
			{
				return ContactsGeneration;
			}
			return EnablePCM ? ContactsGeneration.PersistentContactManifold : ContactsGeneration.LegacyContactsGeneration;
		}
		private IReadOnlyList<uint> GetLayerCollisionMatrix(Version version)
		{
			if(IsReadLayerCollisionMatrix(version))
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
			return IsReadAutoSimulation(version) ? AutoSimulation : true;
		}
		private bool GetAutoSyncTransforms(Version version)
		{
			return IsReadAutoSyncTransforms(version) ? AutoSyncTransforms : true;
		}
		private Vector3f GetClothGravity(Version version)
		{
			return IsReadClothGravity(version) ? ClothGravity : new Vector3f(0.0f, -9.81f, 0.0f);
		}
		private AABB GetWorldBounds(Version version)
		{
			if(IsReadAutoSyncTransforms(version))
			{
				return WorldBounds;
			}
			return new AABB(default, new Vector3f(250.0f, 250.0f, 250.0f));
		}
		private int GetWorldSubdivisions(Version version)
		{
			return IsReadClothInterCollisionSettingsToggle(version) ? WorldSubdivisions : 8;
		}
		private bool GetEnableUnifiedHeightmaps(Version version)
		{
			return IsReadFrictionType(version) ? EnableUnifiedHeightmaps : true;
		}
		private float GetDefaultMaxAngularSpeed(Version version)
		{
			return IsReadDefaultMaxAngularSpeed(version) ? DefaultMaxAngularSpeed : 7.0f;
		}

		public float BounceThreshold { get; private set; }
		public float SleepThreshold { get; private set; }
		public float SleepVelocity { get; private set; }
		public float SleepAngularVelocity { get; private set; }
		public float MaxAngularVelocity { get; private set; }
		public float MinPenetrationForPenalty { get; private set; }
		public float DefaultContactOffset { get; private set; }
		/// <summary>
		/// SolverIterationCount previosuly
		/// </summary>
		public int DefaultSolverIterations { get; private set; }
		/// <summary>
		/// SolverVelocityIterations previously
		/// </summary>
		public int DefaultSolverVelocityIterations { get; private set; }
		public bool QueriesHitBackfaces { get; private set; }
		/// <summary>
		/// RaycastsHitTriggers previosly
		/// </summary>
		public bool QueriesHitTriggers { get; private set; }
		public bool EnableAdaptiveForce { get; private set; }
		public bool EnablePCM { get; private set; }
		public float ClothInterCollisionDistance { get; private set; }
		public float ClothInterCollisionStiffness { get; private set; }
		public ContactsGeneration ContactsGeneration { get; private set; }
		public IReadOnlyList<uint> LayerCollisionMatrix => m_layerCollisionMatrix;
		public bool AutoSimulation { get; private set; }
		public bool AutoSyncTransforms { get; private set; }
		public bool ReuseCollisionCallbacks { get; private set; }
		public bool ClothInterCollisionSettingsToggle { get; private set; }
		public ContactPairsMode ContactPairsMode { get; private set; }
		public BroadphaseType BroadphaseType { get; private set; }
		public int WorldSubdivisions { get; private set; }
		public FrictionType FrictionType { get; private set; }
		public bool EnableEnhancedDeterminism { get; private set; }
		public bool EnableUnifiedHeightmaps { get; private set; }
		/// <summary>
		/// DefaultMaxAngluarSpeed previously
		/// </summary>
		public float DefaultMaxAngularSpeed { get; private set; }

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
		public const string DefaultMaxAngluarSpeedName = "m_DefaultMaxAngluarSpeed";
		public const string DefaultMaxAngularSpeedName = "m_DefaultMaxAngularSpeed";

		public Vector3f Gravity;
		public PPtr<PhysicMaterial> DefaultMaterial;
		public Vector3f ClothGravity;
		public AABB WorldBounds;

		private uint[] m_layerCollisionMatrix;
	}
}
