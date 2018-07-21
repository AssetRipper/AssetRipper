using System.Collections.Generic;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public class SpriteMetaData : IYAMLExportable
	{
		public SpriteMetaData(string name, Rectf rect, int alignment, Vector2f pivot, Vector4f border)
		{
			Name = name;
			Rect = rect;
			Alignment = alignment;
			Pivot = pivot;
			Border = border;

			m_outline = new Vector2f[0];
			m_physicsShape = new Vector2f[0];
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("name", Name);
			node.Add("rect", Rect.ExportYAML(container));
			node.Add("alignment", Alignment);
			node.Add("pivot", Pivot.ExportYAML(container));
			node.Add("border", Border.ExportYAML(container));
			node.Add("outline", Outline.ExportYAML(container));
			node.Add("physicsShape", PhysicsShape.ExportYAML(container));
			node.Add("tessellationDetail", TessellationDetail);
			return node;
		}

		public string Name { get; private set; }
		public int Alignment { get; private set; }
		public IReadOnlyList<Vector2f> Outline => m_outline;
		public IReadOnlyList<Vector2f> PhysicsShape => m_physicsShape;
		public float TessellationDetail { get; private set; }

		public Rectf Rect;
		public Vector2f Pivot;
		public Vector4f Border;

		private Vector2f[] m_outline;
		private Vector2f[] m_physicsShape;
	}
}
