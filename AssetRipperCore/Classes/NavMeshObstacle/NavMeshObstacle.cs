using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.NavMeshObstacle
{
	public sealed class NavMeshObstacle : Behaviour
	{
		public NavMeshObstacle(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// added Shape and Extents
			if (version.IsGreaterEqual(5))
			{
				return 3;
			}
			// 5.0.0a_unknown added 'Vector3f m_Size'
			// return 2;
			return 1;
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasShape(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasMoveThreshold(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasCarveOnlyStationary(UnityVersion version) => version.IsGreaterEqual(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasShape(reader.Version))
			{
				Shape = (NavMeshObstacleShape)reader.ReadInt32();
				Extents.Read(reader);
			}
			else
			{
				float m_Radius = reader.ReadSingle();
				float m_Height = reader.ReadSingle();
				Shape = NavMeshObstacleShape.Capsule;
				Extents = new Vector3f(m_Radius, m_Radius, m_Height);
				Center = new Vector3f(0.0f, m_Height / 2.0f, 0.0f);
			}
			if (HasMoveThreshold(reader.Version))
			{
				MoveThreshold = reader.ReadSingle();
				Carve = reader.ReadBoolean();
			}
			if (HasCarveOnlyStationary(reader.Version))
			{
				CarveOnlyStationary = reader.ReadBoolean();
				reader.AlignStream();

				Center.Read(reader);
				TimeToStationary = reader.ReadSingle();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ShapeName, (int)Shape);
			node.Add(ExtentsName, Extents.ExportYAML(container));
			node.Add(MoveThresholdName, GetMoveThreshold(container.Version));
			node.Add(CarveName, Carve);
			node.Add(CarveOnlyStationaryName, GetCarveOnlyStationary(container.Version));
			node.Add(CenterName, Center.ExportYAML(container));
			node.Add(TimeToStationaryName, GetTimeToStationary(container.Version));
			return node;
		}

		private float GetMoveThreshold(UnityVersion version)
		{
			return HasMoveThreshold(version) ? MoveThreshold : 0.1f;
		}
		private bool GetCarveOnlyStationary(UnityVersion version)
		{
			return HasCarveOnlyStationary(version) ? CarveOnlyStationary : true;
		}
		private float GetTimeToStationary(UnityVersion version)
		{
			return HasCarveOnlyStationary(version) ? TimeToStationary : 0.5f;
		}

		public NavMeshObstacleShape Shape { get; set; }
		public float MoveThreshold { get; set; }
		public bool Carve { get; set; }
		public bool CarveOnlyStationary { get; set; }
		public float TimeToStationary { get; set; }

		public const string ShapeName = "m_Shape";
		public const string ExtentsName = "m_Extents";
		public const string MoveThresholdName = "m_MoveThreshold";
		public const string CarveName = "m_Carve";
		public const string CarveOnlyStationaryName = "m_CarveOnlyStationary";
		public const string CenterName = "m_Center";
		public const string TimeToStationaryName = "m_TimeToStationary";

		public Vector3f Extents = new();
		public Vector3f Center = new();
	}
}
