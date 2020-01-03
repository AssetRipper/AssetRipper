using System;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.Sprites;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TextureImporters
{
	public struct SpriteMetaData : IAsset
	{
		public SpriteMetaData(Version version)
		{
			Name = "sprite_0";
			Rect = default;
			Alignment = SpriteAlignment.Center;
			Pivot = default;
			Border = default;
			Outline = Array.Empty<Vector2f[]>();
			PhysicsShape = Array.Empty<Vector2f[]>();
			TessellationDetail = default;
			Bones = Array.Empty<SpriteBone>();
			SpriteID = string.Empty;
			InternalID = 0;
			Vertices = Array.Empty<Vector2f>();
			Indices = Array.Empty<int>();
			Edges = Array.Empty<Int2Storage>();
			Weights = Array.Empty<BoneWeights4>();
		}

		public static int ToSerializedVersion(Version version)
		{
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 3, 6))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasBorder(Version version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasOutline(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasPhysicsShape(Version version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasTessellationDetail(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasBones(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2019.1. and greater
		/// </summary>
		public static bool HasInternalID(Version version) => version.IsGreaterEqual(2019);

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Rect.Read(reader);
			Alignment = (SpriteAlignment)reader.ReadInt32();
			Pivot.Read(reader);
			if (HasBorder(reader.Version))
			{
				Border.Read(reader);
			}
			if (HasOutline(reader.Version))
			{
				Outline = reader.ReadAssetArrayArray<Vector2f>();
			}
			if (HasPhysicsShape(reader.Version))
			{
				PhysicsShape = reader.ReadAssetArrayArray<Vector2f>();
			}
			if (HasTessellationDetail(reader.Version))
			{
				TessellationDetail = reader.ReadSingle();
			}
			if (HasBones(reader.Version))
			{
				Bones = reader.ReadAssetArray<SpriteBone>();
				SpriteID = reader.ReadString();
			}
			if (HasInternalID(reader.Version))
			{
				InternalID = reader.ReadInt64();
			}
			if (HasBones(reader.Version))
			{
				Vertices = reader.ReadAssetArray<Vector2f>();
				Indices = reader.ReadInt32Array();
				Edges = reader.ReadAssetArray<Int2Storage>();
				reader.AlignStream();

				Weights = reader.ReadAssetArray<BoneWeights4>();
				reader.AlignStream();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Name);
			Rect.Write(writer);
			writer.Write((int)Alignment);
			Pivot.Write(writer);
			if (HasBorder(writer.Version))
			{
				Border.Write(writer);
			}
			if (HasOutline(writer.Version))
			{
				Outline.Write(writer);
			}
			if (HasPhysicsShape(writer.Version))
			{
				PhysicsShape.Write(writer);
			}
			if (HasTessellationDetail(writer.Version))
			{
				writer.Write(TessellationDetail);
			}
			if (HasBones(writer.Version))
			{
				Bones.Write(writer);
				writer.Write(SpriteID);
			}
			if (HasInternalID(writer.Version))
			{
				writer.Write(InternalID);
			}
			if (HasBones(writer.Version))
			{
				Vertices.Write(writer);
				Indices.Write(writer);
				Edges.Write(writer);
				writer.AlignStream();

				Weights.Write(writer);
				writer.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(NameName, Name);
			node.Add(RectName, Rect.ExportYAML(container));
			node.Add(AlignmentName, (int)Alignment);
			node.Add(PivotName, Pivot.ExportYAML(container));
			if (HasBorder(container.ExportVersion))
			{
				node.Add(BorderName, Border.ExportYAML(container));
			}
			if (HasOutline(container.ExportVersion))
			{
				node.Add(OutlineName, Outline.ExportYAML(container));
			}
			if (HasPhysicsShape(container.ExportVersion))
			{
				node.Add(PhysicsShapeName, PhysicsShape.ExportYAML(container));
			}
			if (HasTessellationDetail(container.ExportVersion))
			{
				node.Add(TessellationDetailName, TessellationDetail);
			}
			if (HasBones(container.ExportVersion))
			{
				node.Add(BonesName, Bones.ExportYAML(container));
				node.Add(SpriteIDName, SpriteID);
			}
			if (HasInternalID(container.ExportVersion))
			{
				node.Add(InternalIDName, InternalID);
			}
			if (HasBones(container.ExportVersion))
			{
				node.Add(VerticesName, Vertices.ExportYAML(container));
				node.Add(IndicesName, Indices.ExportYAML(true));
				node.Add(EdgesName, Edges.ExportYAML(container));
				node.Add(WeightsName, Weights.ExportYAML(container));
			}
			return node;
		}

		public string Name { get; set; }
		public SpriteAlignment Alignment { get; set; }
		public Vector2f[][] Outline { get; set; }
		public Vector2f[][] PhysicsShape { get; set; }
		public float TessellationDetail { get; set; }
		public SpriteBone[] Bones { get; set; }
		public string SpriteID { get; set; }
		public long InternalID { get; set; }
		public Vector2f[] Vertices { get; set; }
		public int[] Indices { get; set; }
		public Int2Storage[] Edges { get; set; }
		public BoneWeights4[] Weights { get; set; }

		public const string NameName = "m_Name";
		public const string RectName = "m_Rect";
		public const string AlignmentName = "m_Alignment";
		public const string PivotName = "m_Pivot";
		public const string BorderName = "m_Border";
		public const string OutlineName = "m_Outline";
		public const string PhysicsShapeName = "m_PhysicsShape";
		public const string TessellationDetailName = "m_TessellationDetail";
		public const string BonesName = "m_Bones";
		public const string SpriteIDName = "m_SpriteID";
		public const string InternalIDName = "m_InternalID";
		public const string VerticesName = "m_Vertices";
		public const string IndicesName = "m_Indices";
		public const string EdgesName = "m_Edges";
		public const string WeightsName = "m_Weights";

		public Rectf Rect;
		public Vector2f Pivot;
		public Vector4f Border;
	}
}