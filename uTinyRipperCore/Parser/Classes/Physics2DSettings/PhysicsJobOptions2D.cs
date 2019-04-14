using uTinyRipper.AssetExporters;
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

		private static int GetSerializedVersion(Version version)
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
			reader.AlignStream(AlignType.Align4);

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
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("useMultithreading", UseMultithreading);
			node.Add("useConsistencySorting", UseConsistencySorting);
			node.Add("m_InterpolationPosesPerJob", InterpolationPosesPerJob);
			node.Add("m_NewContactsPerJob", NewContactsPerJob);
			node.Add("m_CollideContactsPerJob", CollideContactsPerJob);
			node.Add("m_ClearFlagsPerJob", ClearFlagsPerJob);
			node.Add("m_ClearBodyForcesPerJob", ClearBodyForcesPerJob);
			node.Add("m_SyncDiscreteFixturesPerJob", SyncDiscreteFixturesPerJob);
			node.Add("m_SyncContinuousFixturesPerJob", SyncContinuousFixturesPerJob);
			node.Add("m_FindNearestContactsPerJob", FindNearestContactsPerJob);
			node.Add("m_UpdateTriggerContactsPerJob", UpdateTriggerContactsPerJob);
			node.Add("m_IslandSolverCostThreshold", IslandSolverCostThreshold);
			node.Add("m_IslandSolverBodyCostScale", IslandSolverBodyCostScale);
			node.Add("m_IslandSolverContactCostScale", IslandSolverContactCostScale);
			node.Add("m_IslandSolverJointCostScale", IslandSolverJointCostScale);
			node.Add("m_IslandSolverBodiesPerJob", IslandSolverBodiesPerJob);
			node.Add("m_IslandSolverContactsPerJob", IslandSolverContactsPerJob);
			return node;
		}

		public bool UseMultithreading { get; private set; }
		public bool UseConsistencySorting { get; private set; }
		public int InterpolationPosesPerJob { get; private set; }
		public int NewContactsPerJob { get; private set; }
		public int CollideContactsPerJob { get; private set; }
		public int ClearFlagsPerJob { get; private set; }
		public int ClearBodyForcesPerJob { get; private set; }
		public int SyncDiscreteFixturesPerJob { get; private set; }
		public int SyncContinuousFixturesPerJob { get; private set; }
		public int FindNearestContactsPerJob { get; private set; }
		public int UpdateTriggerContactsPerJob { get; private set; }
		public int IslandSolverCostThreshold { get; private set; }
		public int IslandSolverBodyCostScale { get; private set; }
		public int IslandSolverContactCostScale { get; private set; }
		public int IslandSolverJointCostScale { get; private set; }
		public int IslandSolverBodiesPerJob { get; private set; }
		public int IslandSolverContactsPerJob { get; private set; }
	}
}
