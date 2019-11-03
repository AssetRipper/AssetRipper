using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Physics2DSettingss
{
	public struct PhysicsJobOptions2D : IAssetReadable, IYAMLExportable
	{
		public PhysicsJobOptions2D(bool _)
		{
			UseMultithreading = false;
			UseConsistencySorting = false;
			InterpolationPosesPerJob = 100;
			NewContactsPerJob = 30;
			CollideContactsPerJob = 100;
			ClearFlagsPerJob = 200;
			ClearBodyForcesPerJob = 200;
			SyncDiscreteFixturesPerJob = 50;
			SyncContinuousFixturesPerJob = 50;
			FindNearestContactsPerJob = 100;
			UpdateTriggerContactsPerJob = 100;
			IslandSolverCostThreshold = 100;
			IslandSolverBodyCostScale = 1;
			IslandSolverContactCostScale = 10;
			IslandSolverJointCostScale = 10;
			IslandSolverBodiesPerJob = 50;
			IslandSolverContactsPerJob = 50;
		}

		public static int ToSerializedVersion(Version version)
		{
			// m_UseMultithreading renamed to useMultithreading, m_UseConsistencySorting to useConsistencySorting
			if (version.IsGreaterEqual(2018, 1, 0, VersionType.Beta, 11))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			UseMultithreading = reader.ReadBoolean();
			UseConsistencySorting = reader.ReadBoolean();
			reader.AlignStream();

			InterpolationPosesPerJob = reader.ReadInt32();
			NewContactsPerJob = reader.ReadInt32();
			CollideContactsPerJob = reader.ReadInt32();
			ClearFlagsPerJob = reader.ReadInt32();
			ClearBodyForcesPerJob = reader.ReadInt32();
			SyncDiscreteFixturesPerJob = reader.ReadInt32();
			SyncContinuousFixturesPerJob = reader.ReadInt32();
			FindNearestContactsPerJob = reader.ReadInt32();
			UpdateTriggerContactsPerJob = reader.ReadInt32();
			IslandSolverCostThreshold = reader.ReadInt32();
			IslandSolverBodyCostScale = reader.ReadInt32();
			IslandSolverContactCostScale = reader.ReadInt32();
			IslandSolverJointCostScale = reader.ReadInt32();
			IslandSolverBodiesPerJob = reader.ReadInt32();
			IslandSolverContactsPerJob = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(UseMultithreadingName, UseMultithreading);
			node.Add(UseConsistencySortingName, UseConsistencySorting);
			node.Add(InterpolationPosesPerJobName, InterpolationPosesPerJob);
			node.Add(NewContactsPerJobName, NewContactsPerJob);
			node.Add(CollideContactsPerJobName, CollideContactsPerJob);
			node.Add(ClearFlagsPerJobName, ClearFlagsPerJob);
			node.Add(ClearBodyForcesPerJobName, ClearBodyForcesPerJob);
			node.Add(SyncDiscreteFixturesPerJobName, SyncDiscreteFixturesPerJob);
			node.Add(SyncContinuousFixturesPerJobName, SyncContinuousFixturesPerJob);
			node.Add(FindNearestContactsPerJobName, FindNearestContactsPerJob);
			node.Add(UpdateTriggerContactsPerJobName, UpdateTriggerContactsPerJob);
			node.Add(IslandSolverCostThresholdName, IslandSolverCostThreshold);
			node.Add(IslandSolverBodyCostScaleName, IslandSolverBodyCostScale);
			node.Add(IslandSolverContactCostScaleName, IslandSolverContactCostScale);
			node.Add(IslandSolverJointCostScaleName, IslandSolverJointCostScale);
			node.Add(IslandSolverBodiesPerJobName, IslandSolverBodiesPerJob);
			node.Add(IslandSolverContactsPerJobName, IslandSolverContactsPerJob);
			return node;
		}

		public bool UseMultithreading { get; set; }
		public bool UseConsistencySorting { get; set; }
		public int InterpolationPosesPerJob { get; set; }
		public int NewContactsPerJob { get; set; }
		public int CollideContactsPerJob { get; set; }
		public int ClearFlagsPerJob { get; set; }
		public int ClearBodyForcesPerJob { get; set; }
		public int SyncDiscreteFixturesPerJob { get; set; }
		public int SyncContinuousFixturesPerJob { get; set; }
		public int FindNearestContactsPerJob { get; set; }
		public int UpdateTriggerContactsPerJob { get; set; }
		public int IslandSolverCostThreshold { get; set; }
		public int IslandSolverBodyCostScale { get; set; }
		public int IslandSolverContactCostScale { get; set; }
		public int IslandSolverJointCostScale { get; set; }
		public int IslandSolverBodiesPerJob { get; set; }
		public int IslandSolverContactsPerJob { get; set; }

		public const string UseMultithreadingName = "useMultithreading";
		public const string UseConsistencySortingName = "useConsistencySorting";
		public const string InterpolationPosesPerJobName = "m_InterpolationPosesPerJob";
		public const string NewContactsPerJobName = "m_NewContactsPerJob";
		public const string CollideContactsPerJobName = "m_CollideContactsPerJob";
		public const string ClearFlagsPerJobName = "m_ClearFlagsPerJob";
		public const string ClearBodyForcesPerJobName = "m_ClearBodyForcesPerJob";
		public const string SyncDiscreteFixturesPerJobName = "m_SyncDiscreteFixturesPerJob";
		public const string SyncContinuousFixturesPerJobName = "m_SyncContinuousFixturesPerJob";
		public const string FindNearestContactsPerJobName = "m_FindNearestContactsPerJob";
		public const string UpdateTriggerContactsPerJobName = "m_UpdateTriggerContactsPerJob";
		public const string IslandSolverCostThresholdName = "m_IslandSolverCostThreshold";
		public const string IslandSolverBodyCostScaleName = "m_IslandSolverBodyCostScale";
		public const string IslandSolverContactCostScaleName = "m_IslandSolverContactCostScale";
		public const string IslandSolverJointCostScaleName = "m_IslandSolverJointCostScale";
		public const string IslandSolverBodiesPerJobName = "m_IslandSolverBodiesPerJob";
		public const string IslandSolverContactsPerJobName = "m_IslandSolverContactsPerJob";
	}
}
