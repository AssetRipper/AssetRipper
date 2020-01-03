using System;
using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.Sprites;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TextureImporters
{
	public struct SpriteSheetMetaData : IAsset, IDependent
	{
		public SpriteSheetMetaData(AssetLayout layout)
		{
			Sprites = Array.Empty<SpriteMetaData>();
			Outline = Array.Empty<Vector2f[]>();
			PhysicsShape = Array.Empty<Vector2f[]>();
			Bones = Array.Empty<SpriteBone>();
			SpriteID = string.Empty;
			InternalID = 0;
			Vertices = Array.Empty<Vector2f>();
			Indices = Array.Empty<int>();
			Edges = Array.Empty<Int2Storage>();
			Weights = Array.Empty<BoneWeights4>();
			SecondaryTextures = Array.Empty<SecondarySpriteTexture>();
		}

		public SpriteSheetMetaData(ref SpriteMetaData metadata)
		{
			Sprites = Array.Empty<SpriteMetaData>();
			Outline = metadata.Outline;
			PhysicsShape = metadata.PhysicsShape;
			Bones = metadata.Bones;
			SpriteID = metadata.SpriteID;
			InternalID = 0;
			Vertices = metadata.Vertices;
			Indices = metadata.Indices;
			Edges = metadata.Edges;
			Weights = metadata.Weights;
			SecondaryTextures = Array.Empty<SecondarySpriteTexture>();
		}

		public static int ToSerializedVersion(Version version)
		{
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 3, 7))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasSecondaryTextures(Version version) => version.IsGreaterEqual(2019);

		public ref SpriteMetaData GetSpriteMetaData(string name)
		{
			for (int i = 0; i < Sprites.Length; i++)
			{
				if (Sprites[i].Name == name)
				{
					return ref Sprites[i];
				}
			}
			throw new ArgumentException($"There is no sprite metadata with name {name}");
		}

		public void Read(AssetReader reader)
		{
			Sprites = reader.ReadAssetArray<SpriteMetaData>();
			if (SpriteMetaData.HasOutline(reader.Version))
			{
				Outline = reader.ReadAssetArrayArray<Vector2f>();
			}
			if (SpriteMetaData.HasPhysicsShape(reader.Version))
			{
				PhysicsShape = reader.ReadAssetArrayArray<Vector2f>();
			}
			if (SpriteMetaData.HasBones(reader.Version))
			{
				Bones = reader.ReadAssetArray<SpriteBone>();
				SpriteID = reader.ReadString();
			}
			if (SpriteMetaData.HasInternalID(reader.Version))
			{
				InternalID = reader.ReadInt64();
			}
			if (SpriteMetaData.HasBones(reader.Version))
			{
				Vertices = reader.ReadAssetArray<Vector2f>();
				Indices = reader.ReadInt32Array();
				Edges = reader.ReadAssetArray<Int2Storage>();
				reader.AlignStream();

				Weights = reader.ReadAssetArray<BoneWeights4>();
				reader.AlignStream();
			}
			if (HasSecondaryTextures(reader.Version))
			{
				SecondaryTextures = reader.ReadAssetArray<SecondarySpriteTexture>();
			}
		}

		public void Write(AssetWriter writer)
		{
			Sprites.Write(writer);
			if (SpriteMetaData.HasOutline(writer.Version))
			{
				Outline.Write(writer);
			}
			if (SpriteMetaData.HasPhysicsShape(writer.Version))
			{
				PhysicsShape.Write(writer);
			}
			if (SpriteMetaData.HasBones(writer.Version))
			{
				Bones.Write(writer);
				writer.Write(SpriteID);
			}
			if (SpriteMetaData.HasInternalID(writer.Version))
			{
				writer.Write(InternalID);
			}
			if (SpriteMetaData.HasBones(writer.Version))
			{
				Vertices.Write(writer);
				Indices.Write(writer);
				Edges.Write(writer);
				writer.AlignStream();

				Weights.Write(writer);
				writer.AlignStream();
			}
			if (HasSecondaryTextures(writer.Version))
			{
				SecondaryTextures.Write(writer);
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in context.FetchDependencies(SecondaryTextures, SecondaryTexturesName))
			{
				yield return asset;
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(SpritesName, Sprites.ExportYAML(container));
			if (SpriteMetaData.HasOutline(container.ExportVersion))
			{
				node.Add(OutlineName, Outline.ExportYAML(container));
			}
			if (SpriteMetaData.HasPhysicsShape(container.ExportVersion))
			{
				node.Add(PhysicsShapeName, PhysicsShape.ExportYAML(container));
			}
			if (SpriteMetaData.HasBones(container.ExportVersion))
			{
				node.Add(BonesName, Bones.ExportYAML(container));
				node.Add(SpriteIDName, SpriteID);
			}
			if (SpriteMetaData.HasInternalID(container.ExportVersion))
			{
				node.Add(InternalIDName, InternalID);
			}
			if (SpriteMetaData.HasBones(container.ExportVersion))
			{
				node.Add(VerticesName, Vertices.ExportYAML(container));
				node.Add(IndicesName, Indices.ExportYAML(true));
				node.Add(EdgesName, Edges.ExportYAML(container));
				node.Add(WeightsName, Weights.ExportYAML(container));
			}
			if (HasSecondaryTextures(container.ExportVersion))
			{
				node.Add(SecondaryTexturesName, SecondaryTextures.ExportYAML(container));
			}
			return node;
		}

		public SpriteMetaData[] Sprites { get; set; }
		public Vector2f[][] Outline { get; set; }
		public Vector2f[][] PhysicsShape { get; set; }
		public SpriteBone[] Bones { get; set; }
		public string SpriteID { get; set; }
		public long InternalID { get; set; }
		public Vector2f[] Vertices { get; set; }
		public int[] Indices { get; set; }
		public Int2Storage[] Edges { get; set; }
		public BoneWeights4[] Weights { get; set; }
		public SecondarySpriteTexture[] SecondaryTextures { get; set; }

		public const string SpritesName = "m_Sprites";
		public const string OutlineName = "m_Outline";
		public const string PhysicsShapeName = "m_PhysicsShape";
		public const string BonesName = "m_Bones";
		public const string SpriteIDName = "m_SpriteID";
		public const string InternalIDName = "m_InternalID";
		public const string VerticesName = "m_Vertices";
		public const string IndicesName = "m_Indices";
		public const string EdgesName = "m_Edges";
		public const string WeightsName = "m_Weights";
		public const string SecondaryTexturesName = "m_SecondaryTextures";
	}
}