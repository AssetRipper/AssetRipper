using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class OffMeshLink : Behaviour
	{
		public OffMeshLink(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// m_NavMeshLayer has been renamed to m_AreaIndex
			if (version.IsGreaterEqual(5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(4))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasAreaIndex(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasAgentTypeID(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasDtPolyRef(Version version) => version.IsLess(5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasAutoUpdatePositions(Version version) => version.IsGreaterEqual(4, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasAreaIndex(reader.Version))
			{
				AreaIndex = reader.ReadUInt32();
			}
			if (HasAgentTypeID(reader.Version))
			{
				AgentTypeID = reader.ReadInt32();
			}
			Start.Read(reader);
			End.Read(reader);
			if (HasDtPolyRef(reader.Version))
			{
				DtPolyRef = reader.ReadUInt32();
			}
			CostOverride = reader.ReadSingle();
			reader.AlignStream();
			
			BiDirectional = reader.ReadBoolean();
			Activated = reader.ReadBoolean();
			if (HasAutoUpdatePositions(reader.Version))
			{
				AutoUpdatePositions = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Start, StartName);
			yield return context.FetchDependency(End, EndName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AreaIndexName, AreaIndex);
			node.Add(AgentTypeIDName, AgentTypeID);
			node.Add(StartName, Start.ExportYAML(container));
			node.Add(EndName, End.ExportYAML(container));
			node.Add(CostOverrideName, CostOverride);
			node.Add(BiDirectionalName, BiDirectional);
			node.Add(ActivatedName, Activated);
			node.Add(AutoUpdatePositionsName, AutoUpdatePositions);
			return node;
		}

		/// <summary>
		/// NavMeshLayer previously
		/// </summary>
		public uint AreaIndex { get; set; }
		public int AgentTypeID { get; set; }
		public uint DtPolyRef { get; set; }
		public float CostOverride { get; set; }
		public bool BiDirectional { get; set; }
		public bool Activated { get; set; }
		public bool AutoUpdatePositions { get; set; }

		public const string NavMeshLayerName = "m_NavMeshLayer";
		public const string AreaIndexName = "m_AreaIndex";
		public const string AgentTypeIDName = "m_AgentTypeID";
		public const string StartName = "m_Start";
		public const string EndName = "m_End";
		public const string DtPolyRefName = "m_DtPolyRef";
		public const string CostOverrideName = "m_CostOverride";
		public const string BiDirectionalName = "m_BiDirectional";
		public const string ActivatedName = "m_Activated";
		public const string AutoUpdatePositionsName = "m_AutoUpdatePositions";

		public PPtr<Transform> Start;
		public PPtr<Transform> End;
	}
}
