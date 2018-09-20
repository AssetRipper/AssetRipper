using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.CompositeCollider2Ds;
using uTinyRipper.Classes.PolygonCollider2Ds;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class CompositeCollider2D : Collider2D
	{
		public CompositeCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			GeometryType = (GeometryType)reader.ReadInt32();
			GenerationType = (GenerationType)reader.ReadInt32();
			EdgeRadius = reader.ReadSingle();
			m_colliderPaths = reader.ReadArray<SubCollider>();
			reader.AlignStream(AlignType.Align4);

			CompositePaths.Read(reader);
			VertexDistance = reader.ReadSingle();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			foreach (SubCollider collider in ColliderPaths)
			{
				foreach(Object @object in collider.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
		}
		
		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_GeometryType", (int)GeometryType);
			node.Add("m_GenerationType", (int)GenerationType);
			node.Add("m_EdgeRadius", EdgeRadius);
			node.Add("m_ColliderPaths", ColliderPaths.ExportYAML(container));
			node.Add("m_CompositePaths", CompositePaths.ExportYAML(container));
			node.Add("m_VertexDistance", VertexDistance);
			return node;
		}
		
		public GeometryType GeometryType { get; private set; }
		public GenerationType GenerationType { get; private set; }
		public float EdgeRadius {get; private set; }
		public IReadOnlyList<SubCollider> ColliderPaths => m_colliderPaths;
		public float VertexDistance { get; private set; }

		public Polygon2D CompositePaths;

		private SubCollider[] m_colliderPaths;
	}
}
