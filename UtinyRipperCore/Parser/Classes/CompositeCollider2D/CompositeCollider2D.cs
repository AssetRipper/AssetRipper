using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.CompositeCollider2Ds;
using UtinyRipper.Classes.PolygonCollider2Ds;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class CompositeCollider2D : Collider2D
	{
		public CompositeCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			GeometryType = (GeometryType)stream.ReadInt32();
			GenerationType = (GenerationType)stream.ReadInt32();
			EdgeRadius = stream.ReadSingle();
			m_colliderPaths = stream.ReadArray<SubCollider>();
			stream.AlignStream(AlignType.Align4);

			CompositePaths.Read(stream);
			VertexDistance = stream.ReadSingle();
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
