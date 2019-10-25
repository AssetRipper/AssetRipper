using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters.Classes
{
	public class SpriteSheetMetaData : IYAMLExportable
	{
		public SpriteSheetMetaData(SpriteMetaData sprite)
		{
			m_sprites = new SpriteMetaData[] { sprite };
			m_outline = (Vector2f[][])sprite.Outline;
			m_physicsShape = new Vector2f[0][];
		}

		public SpriteSheetMetaData(SpriteMetaData[] sprites)
		{
			m_sprites = sprites;
			m_outline = new Vector2f[0][];
			m_physicsShape = new Vector2f[0][];
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
			node.Add("sprites", Sprites.ExportYAML(container));
			node.Add("outline", Outline.ExportYAML(container));
			node.Add("physicsShape", PhysicsShape.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<SpriteMetaData> Sprites => m_sprites;
		public IReadOnlyList<IReadOnlyList<Vector2f>> Outline => m_outline;
		public IReadOnlyList<IReadOnlyList<Vector2f>> PhysicsShape => m_physicsShape;

		private SpriteMetaData[] m_sprites;
		private Vector2f[][] m_outline;
		private Vector2f[][] m_physicsShape;
	}
}
