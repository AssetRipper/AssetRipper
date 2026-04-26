using AssetRipper.Assets.Generics;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.SpriteAtlasData;
using AssetRipper.SourceGenerated.Subclasses.SpriteBone;
using AssetRipper.SourceGenerated.Subclasses.SpriteMetaData;
using AssetRipper.SourceGenerated.Subclasses.SpriteRenderData;
using AssetRipper.SourceGenerated.Subclasses.SpriteVertex;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using AssetRipper.SourceGenerated.Subclasses.Vector2Int;
using System.Buffers.Binary;
using System.Drawing;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SpriteMetaDataExtensions
{
	/// <summary>
	/// Returns the <see cref="SpriteAlignment"/> enum value stored on <paramref name="data"/>.
	/// </summary>
	public static SpriteAlignment GetAlignment(this ISpriteMetaData data)
	{
		return (SpriteAlignment)data.Alignment;
	}

	/// <summary>
	/// Populates <paramref name="instance"/> from <paramref name="sprite"/> and its optional
	/// <paramref name="atlas"/>. All geometric data — outline, physics shape, edges, bones —
	/// is derived and written here.
	/// </summary>
	/// <remarks>
	/// <b>Warning:</b> This method calls <see cref="ScaleAndOffsetBones"/> which mutates the
	/// source sprite's <c>Bones</c> list in-place. Do not call this method more than once
	/// for the same <paramref name="sprite"/> instance — repeated calls will multiply-apply
	/// the PPU scale to bone positions and lengths.
	/// </remarks>
	public static void FillSpriteMetaData(this ISpriteMetaData instance, ISprite sprite, ISpriteAtlas? atlas)
	{
		sprite.GetSpriteCoordinatesInAtlas(atlas, out RectangleF rect, out Vector2 pivot, out Vector4 border);

		instance.Name = sprite.Name;
		instance.Rect.CopyValues(rect);
		instance.Alignment = (int)SpriteAlignment.Custom;
		instance.Pivot.CopyValues(pivot);
		instance.Border?.CopyValues(border);

		if (instance.Has_Outline())
		{
			GenerateOutline(sprite, atlas, rect, pivot, instance.Outline);
		}

		if (instance.Has_PhysicsShape() && sprite.Has_PhysicsShape())
		{
			GeneratePhysicsShape(sprite, atlas, rect, pivot, instance.PhysicsShape);
		}

		// Edges are built from the render-data outline vertices. Only attempt generation
		// when the sprite actually carries vertex data we can read from.
		if (instance.Has_Edges() && sprite.Has_RD() && sprite.RD.Has_Vertices())
		{
			GenerateEdges(sprite, atlas, rect, pivot, instance.Edges);
		}

		instance.TessellationDetail = 0;

		if (instance.Has_Bones() && sprite.Has_Bones() && instance.Has_SpriteID())
		{
			ScaleAndOffsetBones(sprite);

			instance.Bones.Clear();
			instance.Bones.Capacity = sprite.Bones.Count;
			foreach (ISpriteBone bone in sprite.Bones)
			{
				instance.Bones.AddNew().CopyValues(bone);
			}

			// NOTE: The authoritative sprite ID is derived from binary content.
			// We substitute a random GUID as an adequate reconstruction stand-in.
			instance.SpriteID = Guid.NewGuid().ToString("N");

			instance.SetBoneGeometry(sprite);
		}
	}

	// -------------------------------------------------------------------------
	// Private helpers
	// -------------------------------------------------------------------------

	/// <summary>
	/// Scales all bone positions and lengths by the sprite's pixels-per-unit factor,
	/// then translates each root bone so that its origin aligns with the sprite rect centre.
	/// </summary>
	/// <remarks>
	/// <b>This method mutates <paramref name="sprite"/>'s <c>Bones</c> collection directly.</b>
	/// It must be invoked exactly once per sprite instance. See the warning on
	/// <see cref="FillSpriteMetaData"/>.
	/// </remarks>
	private static void ScaleAndOffsetBones(ISprite sprite)
	{
		float halfWidth  = sprite.Rect.Width  / 2f;
		float halfHeight = sprite.Rect.Height / 2f;

		foreach (ISpriteBone bone in sprite.Bones)
		{
			bone.Position.Scale(sprite.PixelsToUnits);
			bone.Length *= sprite.PixelsToUnits;

			if (bone.ParentId == -1)
			{
				bone.Position.X += halfWidth;
				bone.Position.Y += halfHeight;
			}
		}
	}

	/// <summary>
	/// Writes the mesh vertices, triangle indices, and skin weights that belong to the
	/// sprite's bone-driven geometry into <paramref name="instance"/>.
	/// </summary>
	private static void SetBoneGeometry(this ISpriteMetaData instance, ISprite origin)
	{
		Vector3[]?     vertices = null;
		BoneWeight4[]? skin     = null;

		if (origin.RD.Has_VertexData())
		{
			VertexDataBlob
				.Create(origin.RD.VertexData, origin.Collection.Version, origin.Collection.EndianType)
				.ReadData(
					out vertices,
					out Vector3[]?    _,  // normals  — not needed for sprite reconstruction
					out Vector4[]?    _,  // tangents — not needed for sprite reconstruction
					out ColorFloat[]? _,  // colors   — not needed for sprite reconstruction
					out Vector2[]?    _,  // uv0      — not needed for sprite reconstruction
					out Vector2[]?    _,  // uv1      — not needed for sprite reconstruction
					out Vector2[]?    _,  // uv2      — not needed for sprite reconstruction
					out Vector2[]?    _,  // uv3      — not needed for sprite reconstruction
					out Vector2[]?    _,  // uv4      — not needed for sprite reconstruction
					out Vector2[]?    _,  // uv5      — not needed for sprite reconstruction
					out Vector2[]?    _,  // uv6      — not needed for sprite reconstruction
					out Vector2[]?    _,  // uv7      — not needed for sprite reconstruction
					out skin);
		}

		// ------------------------------------------------------------------
		// Vertices — project 3-D mesh positions onto the 2-D sprite plane,
		// apply the PPU scale, and offset by the rect half-extents so the
		// result is in sprite-space pixels from the trimmed rect origin.
		// ------------------------------------------------------------------
		if (instance.Has_Vertices())
		{
			instance.Vertices.Clear();

			if (vertices is null)
			{
				instance.Vertices.Capacity = 0;
			}
			else
			{
				float halfWidth  = origin.Rect.Width  / 2f;
				float halfHeight = origin.Rect.Height / 2f;

				instance.Vertices.Capacity = vertices.Length;
				for (int i = 0; i < vertices.Length; i++)
				{
					Vector2f vertex = instance.Vertices.AddNew();
					vertex.X = vertices[i].X * origin.PixelsToUnits + halfWidth;
					vertex.Y = vertices[i].Y * origin.PixelsToUnits + halfHeight;
				}
			}
		}

		// ------------------------------------------------------------------
		// Indices — read 16-bit little-endian values from the raw index buffer.
		// Has_IndexBuffer() MUST be checked before accessing IndexBuffer; on
		// older Unity versions the property can throw or return an empty array
		// when the format version does not carry the field.
		// ------------------------------------------------------------------
		if (instance.Has_Indices())
		{
			instance.Indices.Clear();

			if (origin.RD.Has_IndexBuffer())
			{
				ReadOnlySpan<byte> indexBuffer = origin.RD.IndexBuffer;
				if (indexBuffer.Length != 0)
				{
					int indexCount = indexBuffer.Length / sizeof(short);
					instance.Indices.Capacity = indexCount;

					for (int i = 0; i < indexCount; i++)
					{
						instance.Indices.Add(
							BinaryPrimitives.ReadInt16LittleEndian(
								indexBuffer.Slice(i * sizeof(short), sizeof(short))));
					}
				}
			}
		}

		// Edge reconstruction for bone-animated sprites is deferred. The mesh edge topology
		// is not independently stored for bone sprites and would need to be re-derived from
		// the index buffer — a non-trivial operation left for a follow-up pass.

		// ------------------------------------------------------------------
		// Skin weights
		// ------------------------------------------------------------------
		if (instance.Has_Weights())
		{
			instance.Weights.Clear();
			if (skin is not null)
			{
				instance.Weights.EnsureCapacity(skin.Length);
				for (int i = 0; i < skin.Length; i++)
				{
					instance.Weights.AddNew().CopyValues(skin[i]);
				}
			}
		}
	}

	/// <summary>
	/// Generates physics-shape contours from the sprite's own physics shape data,
	/// transforms them into sprite space, and applies any atlas packing rotation.
	/// </summary>
	private static void GeneratePhysicsShape(
		ISprite sprite,
		ISpriteAtlas? atlas,
		RectangleF rect,
		Vector2 pivot,
		AssetList<AssetList<Vector2f>> shape)
	{
		if (!sprite.Has_PhysicsShape() || sprite.PhysicsShape.Count == 0)
		{
			return;
		}

		shape.Clear();
		shape.Capacity = sprite.PhysicsShape.Count;

		Vector2 pivotShift = ComputePivotShift(rect, pivot);

		for (int i = 0; i < sprite.PhysicsShape.Count; i++)
		{
			AssetList<Vector2f> sourceList = sprite.PhysicsShape[i];
			AssetList<Vector2f> targetList = shape.AddNew();
			targetList.Capacity = sourceList.Count;

			for (int j = 0; j < sourceList.Count; j++)
			{
				Vector2 point = (Vector2)sourceList[j] * sprite.PixelsToUnits;
				targetList.AddNew().CopyValues(point + pivotShift);
			}
		}

		shape.FixRotation(sprite, atlas);
	}

	/// <summary>
	/// Generates edge entries for the sprite from its outline contour vertices, transforms
	/// them into sprite space, and applies atlas packing rotation. Coordinates are truncated
	/// to integers (sprite pixels).
	/// </summary>
	private static void GenerateEdges(
		ISprite sprite,
		ISpriteAtlas? atlas,
		RectangleF rect,
		Vector2 pivot,
		AssetList<Vector2Int> edges)
	{
		AssetList<AssetList<Vector2f>> outlines = new();
		GenerateOutline(sprite.RD, sprite.Collection.Version, outlines);

		// Pre-count total points across all contours to avoid repeated list resizing.
		int totalPoints = 0;
		foreach (AssetList<Vector2f> outline in outlines)
		{
			totalPoints += outline.Count;
		}

		edges.Clear();
		edges.Capacity = totalPoints;

		Vector2 pivotShift = ComputePivotShift(rect, pivot);

		foreach (AssetList<Vector2f> outline in outlines)
		{
			for (int i = 0; i < outline.Count; i++)
			{
				Vector2 point = (Vector2)outline[i] * sprite.PixelsToUnits + pivotShift;
				Vector2Int edge = edges.AddNew();
				edge.m_X = (int)point.X;
				edge.m_Y = (int)point.Y;
			}
		}

		GetPacking(sprite, atlas, out bool isPacked, out SpritePackingRotation rotation);
		if (!isPacked)
		{
			return;
		}

		switch (rotation)
		{
			case SpritePackingRotation.FlipHorizontal:
				for (int i = 0; i < edges.Count; i++)
				{
					edges[i].m_X = -edges[i].m_X;
				}
				break;

			case SpritePackingRotation.FlipVertical:
				for (int i = 0; i < edges.Count; i++)
				{
					edges[i].m_Y = -edges[i].m_Y;
				}
				break;

			case SpritePackingRotation.Rotate90:
				// Cache via a local to avoid indexing the list four times and to make the
				// swap semantics immediately obvious to the reader.
				for (int i = 0; i < edges.Count; i++)
				{
					Vector2Int edge = edges[i];
					int tmp  = edge.m_X;
					edge.m_X = edge.m_Y;
					edge.m_Y = tmp;
				}
				break;

			case SpritePackingRotation.Rotate180:
				for (int i = 0; i < edges.Count; i++)
				{
					edges[i].m_X = -edges[i].m_X;
					edges[i].m_Y = -edges[i].m_Y;
				}
				break;
		}
	}

	/// <summary>
	/// Applies an atlas packing rotation in-place to a set of outline contours.
	/// </summary>
	private static void FixRotation(this AssetList<AssetList<Vector2f>> outlines, ISprite sprite, ISpriteAtlas? atlas)
	{
		GetPacking(sprite, atlas, out bool isPacked, out SpritePackingRotation rotation);

		if (!isPacked)
		{
			return;
		}

		switch (rotation)
		{
			case SpritePackingRotation.FlipHorizontal:
				foreach (AssetList<Vector2f> outline in outlines)
				{
					for (int i = 0; i < outline.Count; i++)
					{
						outline[i].SetValues(-outline[i].X, outline[i].Y);
					}
				}
				break;

			case SpritePackingRotation.FlipVertical:
				foreach (AssetList<Vector2f> outline in outlines)
				{
					for (int i = 0; i < outline.Count; i++)
					{
						outline[i].SetValues(outline[i].X, -outline[i].Y);
					}
				}
				break;

			case SpritePackingRotation.Rotate90:
				foreach (AssetList<Vector2f> outline in outlines)
				{
					for (int i = 0; i < outline.Count; i++)
					{
						// Both components are read before SetValues is called, so there is
						// no aliasing hazard here.
						outline[i].SetValues(outline[i].Y, outline[i].X);
					}
				}
				break;

			case SpritePackingRotation.Rotate180:
				foreach (AssetList<Vector2f> outline in outlines)
				{
					for (int i = 0; i < outline.Count; i++)
					{
						outline[i].SetValues(-outline[i].X, -outline[i].Y);
					}
				}
				break;
		}
	}

	/// <summary>
	/// Reads packing metadata from the atlas entry (preferred) or falls back to the sprite's
	/// own render data. This method is pure — it does not mutate any state.
	/// </summary>
	private static void GetPacking(
		ISprite sprite,
		ISpriteAtlas? atlas,
		out bool isPacked,
		out SpritePackingRotation rotation)
	{
		if (atlas is not null
		    && sprite.Has_RenderDataKey()
		    && atlas.RenderDataMap.TryGetValue(sprite.RenderDataKey, out ISpriteAtlasData? atlasData))
		{
			isPacked = atlasData.IsPacked;
			rotation = atlasData.PackingRotation;
		}
		else
		{
			isPacked = sprite.RD.IsPacked;
			rotation = sprite.RD.PackingRotation;
		}
	}

	/// <summary>
	/// Generates outline contours for <paramref name="sprite"/>, converts them to sprite
	/// space (PPU scale + pivot shift), and applies atlas packing rotation.
	/// </summary>
	private static void GenerateOutline(
		ISprite sprite,
		ISpriteAtlas? atlas,
		RectangleF rect,
		Vector2 pivot,
		AssetList<AssetList<Vector2f>> outlines)
	{
		GenerateOutline(sprite.RD, sprite.Collection.Version, outlines);

		Vector2 pivotShift = ComputePivotShift(rect, pivot);

		foreach (AssetList<Vector2f> outline in outlines)
		{
			for (int i = 0; i < outline.Count; i++)
			{
				Vector2 point = (Vector2)outline[i] * sprite.PixelsToUnits;
				outline[i].CopyValues(point + pivotShift);
			}
		}

		outlines.FixRotation(sprite, atlas);
	}

	/// <summary>
	/// Builds raw outline contours directly from <paramref name="spriteRenderData"/> without
	/// any scaling or rotation applied. Results are in raw mesh-space units.
	/// </summary>
	private static void GenerateOutline(
		ISpriteRenderData spriteRenderData,
		UnityVersion version,
		AssetList<AssetList<Vector2f>> outlines)
	{
		outlines.Clear();

		if (spriteRenderData.Has_VertexData()
		    && spriteRenderData.SubMeshes is not null
		    && spriteRenderData.SubMeshes.Count != 0)
		{
			for (int i = 0; i < spriteRenderData.SubMeshes.Count; i++)
			{
				Vector3[] vertices = spriteRenderData.VertexData.GenerateVertices(version, spriteRenderData.SubMeshes[i]);
				List<Vector2[]> vectorArrayList = VertexDataToOutline(spriteRenderData.IndexBuffer, vertices, spriteRenderData.SubMeshes[i]);
				outlines.AddRanges(vectorArrayList);
			}
		}
		// Legacy path: per-vertex data stored directly on the render data (pre-5.6 era).
		else if (spriteRenderData.Has_Vertices() && spriteRenderData.Vertices.Count != 0)
		{
			List<Vector2[]> vectorArrayList = VerticesToOutline(spriteRenderData.Vertices, spriteRenderData.Indices);
			outlines.Capacity = vectorArrayList.Count;
			outlines.AddRanges(vectorArrayList);
		}
	}

	/// <summary>
	/// Builds outline contours from legacy per-vertex sprite data (pre-VertexData era).
	/// </summary>
	private static List<Vector2[]> VerticesToOutline(
		AccessListBase<ISpriteVertex> spriteVertexList,
		AssetList<ushort> spriteIndexArray)
	{
		Vector3[] vertices = new Vector3[spriteVertexList.Count];
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = spriteVertexList[i].Pos;
		}

		Vector3i[] triangles = new Vector3i[spriteIndexArray.Count / 3];
		for (int i = 0, j = 0; i < triangles.Length; i++)
		{
			int x = spriteIndexArray[j];
			int y = spriteIndexArray[j + 1];
			int z = spriteIndexArray[j + 2];
			j += 3;
			
			triangles[i] = new Vector3i(x, y, z);
		}

		return new MeshOutlineGenerator(vertices, triangles).GenerateOutlines();
	}

	/// <summary>
	/// Builds outline contours from a packed vertex buffer and its associated index buffer,
	/// scoped to a single sub-mesh.
	/// </summary>
	private static List<Vector2[]> VertexDataToOutline(ReadOnlySpan<byte> indexBuffer, Vector3[] vertices, ISubMesh submesh)
	{
		Vector3i[] triangles = new Vector3i[submesh.IndexCount / 3];
		for (int o = (int)submesh.FirstByte, ti = 0; ti < triangles.Length; o += 3 * sizeof(ushort), ti++)
		{
			ushort x = BinaryPrimitives.ReadUInt16LittleEndian(indexBuffer[o..]);
			ushort y = BinaryPrimitives.ReadUInt16LittleEndian(indexBuffer[(o + sizeof(ushort))..]);
			ushort z = BinaryPrimitives.ReadUInt16LittleEndian(indexBuffer[(o + 2 * sizeof(ushort))..]);
			triangles[ti] = new Vector3i(x, y, z);
		}

		return new MeshOutlineGenerator(vertices, triangles).GenerateOutlines();
	}

	/// <summary>
	/// Appends all contours from <paramref name="vectorArrayList"/> into <paramref name="instance"/>.
	/// </summary>
	private static void AddRanges(this AssetList<AssetList<Vector2f>> instance, List<Vector2[]> vectorArrayList)
	{
		foreach (Vector2[] vectorArray in vectorArrayList)
		{
			AssetList<Vector2f> assetList = instance.AddNew();
			assetList.Capacity = vectorArray.Length;
			foreach (Vector2 v in vectorArray)
			{
				assetList.AddNew().CopyValues(v);
			}
		}
	}

	/// <summary>
	/// Computes the 2-D pivot shift in sprite-space units needed to convert mesh-centred
	/// coordinates into pivot-relative coordinates. Shared by outline, physics-shape, and
	/// edge generation to eliminate duplicated arithmetic.
	/// </summary>
	private static Vector2 ComputePivotShift(RectangleF rect, Vector2 pivot)
	{
		// Factored form: (pivot - 0.5) * size — one fewer multiplication per axis vs the
		// expanded form (size * pivot - size * 0.5).
		return new Vector2(
			rect.Width  * (pivot.X - 0.5f),
			rect.Height * (pivot.Y - 0.5f));
	}
}
