﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System;


namespace AssetRipper.Core.Classes.Meta.Importers.Texture
{
	public sealed class SpriteMetaData : IAsset
	{
		public SpriteMetaData() { }

		public SpriteMetaData(UnityVersion version)
		{
			Name = "sprite_0";
			Rect = new();
			Alignment = SpriteAlignment.Center;
			Pivot = new();
			Border = new();
			Outline = Array.Empty<Vector2f[]>();
			PhysicsShape = Array.Empty<Vector2f[]>();
			TessellationDetail = new();
			Bones = Array.Empty<SpriteBone>();
			SpriteID = string.Empty;
			InternalID = 0;
			Vertices = Array.Empty<Vector2f>();
			Indices = Array.Empty<int>();
			Edges = Array.Empty<Int2Storage>();
			Weights = Array.Empty<BoneWeights4>();
		}

		public static int ToSerializedVersion(UnityVersion version)
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
		public static bool HasBorder(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasOutline(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasPhysicsShape(UnityVersion version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasTessellationDetail(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasBones(UnityVersion version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 2019.1. and greater
		/// </summary>
		public static bool HasInternalID(UnityVersion version) => version.IsGreaterEqual(2019);

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(NameName, Name);
			node.Add(RectName, Rect.ExportYaml(container));
			node.Add(AlignmentName, (int)Alignment);
			node.Add(PivotName, Pivot.ExportYaml(container));
			if (HasBorder(container.ExportVersion))
			{
				node.Add(BorderName, Border.ExportYaml(container));
			}
			if (HasOutline(container.ExportVersion))
			{
				node.Add(OutlineName, Outline.ExportYaml(container));
			}
			if (HasPhysicsShape(container.ExportVersion))
			{
				node.Add(PhysicsShapeName, PhysicsShape.ExportYaml(container));
			}
			if (HasTessellationDetail(container.ExportVersion))
			{
				node.Add(TessellationDetailName, TessellationDetail);
			}
			if (HasBones(container.ExportVersion))
			{
				node.Add(BonesName, Bones.ExportYaml(container));
				node.Add(SpriteIDName, SpriteID);
			}
			if (HasInternalID(container.ExportVersion))
			{
				node.Add(InternalIDName, InternalID);
			}
			if (HasBones(container.ExportVersion))
			{
				node.Add(VerticesName, Vertices.ExportYaml(container));
				node.Add(IndicesName, Indices.ExportYaml(true));
				node.Add(EdgesName, Edges.ExportYaml(container));
				node.Add(WeightsName, Weights.ExportYaml(container));
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

		public Rectf Rect = new();
		public Vector2f Pivot = new();
		public Vector4f Border = new();
	}
}
