using System.Collections.Generic;
using UtinyRipper.Classes;
using UtinyRipper.Classes.SpriteAtlases;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public class SpriteMetaData : IYAMLExportable
	{
		public SpriteMetaData(Sprite sprite)
		{
			Name = sprite.Name;
			Alignment = SpriteAlignment.Custom;

			Vector2f rectOffset;
			SpriteAtlas atlas = sprite.SpriteAtlas.FindObject(sprite.File);
			if(atlas == null)
			{
				Rectf textureRect = sprite.RD.TextureRect;
				Vector2f textureOffset = sprite.RD.TextureRect.Position + Rect.Position;
				Vector2f textureSize = sprite.RD.TextureRect.Size;
				Rect = new Rectf(textureOffset, textureSize);
				rectOffset = sprite.RD.TextureRectOffset;
			}
			else
			{			
				SpriteAtlasData atlasData = atlas.RenderDataMap[sprite.RenderDataKey];
				Vector2f textureOffset = atlasData.TextureRect.Position + Rect.Position;
				Vector2f textureSize = atlasData.TextureRect.Size;
				Rect = new Rectf(textureOffset, textureSize);
				rectOffset = atlasData.TextureRectOffset;
			}
			
			Vector2f decSizeDif = sprite.Rect.Size - Rect.Size;
			Vector2f pivotShiftSize = new Vector2f(sprite.Pivot.X * decSizeDif.X, sprite.Pivot.Y * decSizeDif.Y);
			Vector2f relPivotShiftPos = new Vector2f(rectOffset.X / Rect.Size.X, rectOffset.Y / Rect.Size.Y);
			Vector2f relPivotShiftSize = new Vector2f(pivotShiftSize.X / Rect.Size.X, pivotShiftSize.Y / Rect.Size.Y);

			Pivot = sprite.Pivot - relPivotShiftPos + relPivotShiftSize;

			float borderL = sprite.Border.X == 0.0f ? 0.0f : sprite.Border.X - rectOffset.X;
			float borderB = sprite.Border.Y == 0.0f ? 0.0f : sprite.Border.Y - rectOffset.Y;
			float borderR = sprite.Border.Z == 0.0f ? 0.0f : sprite.Border.Z + rectOffset.X - decSizeDif.X;
			float borderT = sprite.Border.W == 0.0f ? 0.0f : sprite.Border.W + rectOffset.Y - decSizeDif.Y;
			Border = new Vector4f(borderL, borderB, borderR, borderT);

			Outline = sprite.GenerateOutline(Rect, Pivot);
			PhysicsShape = sprite.GeneratePhysicsShape();
			TessellationDetail = 0;
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
