using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.CompositeCollider2Ds;
using uTinyRipper.Classes.PolygonCollider2Ds;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// SpriteCollider2D previously
	/// </summary>
	public sealed class CompositeCollider2D : Collider2D
	{
		public CompositeCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static bool IsReadOffsetDistance(Version version)
		{
			return version.IsGreaterEqual(2019, 1, 3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			GeometryType = (GeometryType)reader.ReadInt32();
			GenerationType = (GenerationType)reader.ReadInt32();
			EdgeRadius = reader.ReadSingle();
			m_colliderPaths = reader.ReadAssetArray<SubCollider>();
			reader.AlignStream(AlignType.Align4);

			CompositePaths.Read(reader);
			VertexDistance = reader.ReadSingle();
			if (IsReadOffsetDistance(reader.Version))
			{
				OffsetDistance = reader.ReadSingle();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (SubCollider collider in ColliderPaths)
			{
				foreach(Object asset in collider.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}
		
		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(GeometryTypeName, (int)GeometryType);
			node.Add(GenerationTypeName, (int)GenerationType);
			node.Add(EdgeRadiusName, EdgeRadius);
			node.Add(ColliderPathsName, ColliderPaths.ExportYAML(container));
			node.Add(CompositePathsName, CompositePaths.ExportYAML(container));
			node.Add(VertexDistanceName, VertexDistance);
			if (IsReadOffsetDistance(container.ExportVersion))
			{
				node.Add(OffsetDistanceName, GetOffsetDistance(container.Version));
			}
			return node;
		}
		
		private float GetOffsetDistance(Version version)
		{
			return IsReadOffsetDistance(version) ? OffsetDistance : 0.000005f;
		}

		public GeometryType GeometryType { get; private set; }
		public GenerationType GenerationType { get; private set; }
		public float EdgeRadius {get; private set; }
		public IReadOnlyList<SubCollider> ColliderPaths => m_colliderPaths;
		public float VertexDistance { get; private set; }
		public float OffsetDistance { get; private set; }

		public const string GeometryTypeName = "m_GeometryType";
		public const string GenerationTypeName = "m_GenerationType";
		public const string EdgeRadiusName = "m_EdgeRadius";
		public const string ColliderPathsName = "m_ColliderPaths";
		public const string CompositePathsName = "m_CompositePaths";
		public const string VertexDistanceName = "m_VertexDistance";
		public const string OffsetDistanceName = "m_OffsetDistance";

		public Polygon2D CompositePaths;

		private SubCollider[] m_colliderPaths;
	}
}
