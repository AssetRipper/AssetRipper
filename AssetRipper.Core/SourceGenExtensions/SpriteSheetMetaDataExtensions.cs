using AssetRipper.SourceGenerated.Subclasses.BoneWeights4;
using AssetRipper.SourceGenerated.Subclasses.SpriteBone;
using AssetRipper.SourceGenerated.Subclasses.SpriteMetaData;
using AssetRipper.SourceGenerated.Subclasses.SpriteSheetMetaData;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using AssetRipper.SourceGenerated.Subclasses.Vector2Int;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SpriteSheetMetaDataExtensions
	{
		public static ISpriteMetaData GetSpriteMetaData(this ISpriteSheetMetaData data, string name)
		{
			for (int i = 0; i < data.Sprites.Count; i++)
			{
				if (data.Sprites[i].Name == name)
				{
					return data.Sprites[i];
				}
			}
			throw new ArgumentException($"There is no sprite metadata with name {name}", nameof(name));
		}

		public static void CopyFromSpriteMetaData(this ISpriteSheetMetaData instance, ISpriteMetaData spriteMetaData)
		{
			if (instance.Has_Outline() && spriteMetaData.Has_Outline())
			{
				instance.Outline.Clear();
				instance.Outline.AddRange(spriteMetaData.Outline);
			}
			if (instance.Has_PhysicsShape() && spriteMetaData.Has_PhysicsShape())
			{
				instance.PhysicsShape.Clear();
				instance.PhysicsShape.AddRange(spriteMetaData.PhysicsShape);
			}
			if (instance.Has_Bones() && spriteMetaData.Has_Bones())
			{
				instance.Bones.Clear();
				instance.Bones.Capacity = spriteMetaData.Bones.Count;
				foreach (ISpriteBone bone in spriteMetaData.Bones)
				{
					instance.Bones.AddNew().CopyValues(bone);
				}
			}
			if (instance.Has_SpriteID() && spriteMetaData.Has_SpriteID())
			{
				instance.SpriteID.CopyValues(instance.SpriteID);
			}
			if (instance.Has_Vertices() && spriteMetaData.Has_Vertices())
			{
				instance.Vertices.Clear();
				instance.Vertices.Capacity = spriteMetaData.Vertices.Count;
				foreach (Vector2f_3_5_0_f5 vertex in spriteMetaData.Vertices)
				{
					instance.Vertices.AddNew().CopyValues(vertex);
				}
			}
			if (instance.Has_Indices() && spriteMetaData.Has_Indices())
			{
				instance.Indices = spriteMetaData.Indices.ToArray();
			}
			if (instance.Has_Edges() && spriteMetaData.Has_Edges())
			{
				instance.Edges.Clear();
				instance.Edges.Capacity = spriteMetaData.Edges.Count;
				foreach (Vector2Int edge in spriteMetaData.Edges)
				{
					instance.Edges.AddNew().CopyValues(edge);
				}
			}
			if (instance.Has_Weights() && spriteMetaData.Has_Weights())
			{
				instance.Weights.Clear();
				instance.Weights.Capacity = spriteMetaData.Weights.Count;
				foreach (IBoneWeights4 weight in spriteMetaData.Weights)
				{
					instance.Weights.AddNew().CopyValues(weight);
				}
			}
		}
	}
}
