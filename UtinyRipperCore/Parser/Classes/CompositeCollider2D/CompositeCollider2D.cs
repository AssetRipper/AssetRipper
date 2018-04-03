using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.CompositeCollider2Ds;
using UtinyRipper.Classes.PolygonCollider2Ds;
using UtinyRipper.Exporter.YAML;

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
		
		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_GeometryType", (int)GeometryType);
			node.Add("m_GenerationType", (int)GenerationType);
			node.Add("m_EdgeRadius", EdgeRadius);
			node.Add("m_ColliderPaths", ColliderPaths.ExportYAML(exporter));
			node.Add("m_CompositePaths", CompositePaths.ExportYAML(exporter));
			node.Add("m_VertexDistance", VertexDistance);
			return node;
		}
		
		public override string ClassIDName => "CompositeCollider2D";

		public GeometryType GeometryType { get; private set; }
		public GenerationType GenerationType { get; private set; }
		public float EdgeRadius {get; private set; }
		public IReadOnlyList<SubCollider> ColliderPaths => m_colliderPaths;
		public float VertexDistance { get; private set; }

		public Polygon2D CompositePaths;

		private SubCollider[] m_colliderPaths;
	}
}
