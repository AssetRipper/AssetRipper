using uTinyRipper.Classes.NavMeshAgents;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class NavMeshAgent : Behaviour
	{
		public NavMeshAgent(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasAgentTypeID(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasAvoidancePriority(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool HasAutoBraking(Version version) => version.IsGreaterEqual(4, 1);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasAgentTypeID(reader.Version))
			{
				AgentTypeID = reader.ReadInt32();
			}
			Radius = reader.ReadSingle();
			Speed = reader.ReadSingle();
			Acceleration = reader.ReadSingle();
			if (HasAvoidancePriority(reader.Version))
			{
				AvoidancePriority = reader.ReadInt32();
			}
			AngularSpeed = reader.ReadSingle();
			StoppingDistance = reader.ReadSingle();
			AutoTraverseOffMeshLink = reader.ReadBoolean();
			if (HasAutoBraking(reader.Version))
			{
				AutoBraking = reader.ReadBoolean();
			}
			AutoRepath = reader.ReadBoolean();
			reader.AlignStream();
			
			Height = reader.ReadSingle();
			BaseOffset = reader.ReadSingle();
			WalkableMask = reader.ReadUInt32();
			ObstacleAvoidanceType = (ObstacleAvoidanceType)reader.ReadInt32();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(AgentTypeIDName, AgentTypeID);
			node.Add(RadiusName, Radius);
			node.Add(SpeedName, Speed);
			node.Add(AccelerationName, Acceleration);
			node.Add(AvoidancePriorityName, GetAvoidancePriority(container.Version));
			node.Add(AngularSpeedName, AngularSpeed);
			node.Add(StoppingDistanceName, StoppingDistance);
			node.Add(AutoTraverseOffMeshLinkName, AutoTraverseOffMeshLink);
			node.Add(AutoBrakingName, GetAutoBraking(container.Version));
			node.Add(AutoRepathName, AutoRepath);
			node.Add(HeightName, Height);
			node.Add(BaseOffsetName, BaseOffset);
			node.Add(WalkableMaskName, WalkableMask);
			node.Add(ObstacleAvoidanceTypeName, (int)ObstacleAvoidanceType);
			return node;
		}

		private float GetAvoidancePriority(Version version)
		{
			return HasAvoidancePriority(version) ? AvoidancePriority : 50.0f;
		}
		private bool GetAutoBraking(Version version)
		{
			return HasAutoBraking(version) ? AutoBraking : true;
		}

		public int AgentTypeID { get; set; }
		public float Radius { get; set; }
		public float Speed { get; set; }
		public float Acceleration { get; set; }
		public int AvoidancePriority { get; set; }
		public float AngularSpeed { get; set; }
		public float StoppingDistance { get; set; }
		public bool AutoTraverseOffMeshLink { get; set; }
		public bool AutoBraking { get; set; }
		public bool AutoRepath { get; set; }
		public float Height { get; set; }
		public float BaseOffset { get; set; }
		public uint WalkableMask { get; set; }
		public ObstacleAvoidanceType ObstacleAvoidanceType { get; set; }

		public const string AgentTypeIDName = "m_AgentTypeID";
		public const string RadiusName = "m_Radius";
		public const string SpeedName = "m_Speed";
		public const string AccelerationName = "m_Acceleration";
		public const string AvoidancePriorityName = "avoidancePriority";
		public const string AngularSpeedName = "m_AngularSpeed";
		public const string StoppingDistanceName = "m_StoppingDistance";
		public const string AutoTraverseOffMeshLinkName = "m_AutoTraverseOffMeshLink";
		public const string AutoBrakingName = "m_AutoBraking";
		public const string AutoRepathName = "m_AutoRepath";
		public const string HeightName = "m_Height";
		public const string BaseOffsetName = "m_BaseOffset";
		public const string WalkableMaskName = "m_WalkableMask";
		public const string ObstacleAvoidanceTypeName = "m_ObstacleAvoidanceType";
	}
}
