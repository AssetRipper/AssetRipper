using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters.Classes
{
	public class SpriteMetaData : IYAMLExportable
	{
		public SpriteMetaData(Sprite sprite, SpriteAtlas atlas)
		{
			Name = sprite.Name;
			Alignment = SpriteAlignment.Custom;

			sprite.GetExportPosition(atlas, out Rectf rect, out Vector2f pivot, out Vector4f border);
			Rect = rect;
			Pivot = pivot;
			Border = border;
			Outline = sprite.GenerateOutline(atlas, Rect, Pivot);
			PhysicsShape = sprite.GeneratePhysicsShape(atlas, Rect, Pivot);
			TessellationDetail = 0;
		}

		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("name", Name);
			node.Add("rect", Rect.ExportYAML(container));
			node.Add("alignment", (int)Alignment);
			node.Add("pivot", Pivot.ExportYAML(container));
			node.Add("border", Border.ExportYAML(container));
			node.Add("outline", Outline.ExportYAML(container));
			node.Add("physicsShape", PhysicsShape.ExportYAML(container));
			node.Add("tessellationDetail", TessellationDetail);
			return node;
		}

		public string Name { get; private set; }
		public SpriteAlignment Alignment { get; private set; }
		public IReadOnlyList<IReadOnlyList<Vector2f>> Outline;
		public IReadOnlyList<IReadOnlyList<Vector2f>> PhysicsShape;
		public float TessellationDetail { get; private set; }

		public Rectf Rect;
		public Vector2f Pivot;
		public Vector4f Border;
	}
}
