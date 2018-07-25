using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes.Meshes;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.Sprites
{
	public struct SpriteRenderData : IAssetReadable, IDependent
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadAlphaTexture(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Less than 5.6.0
		/// </summary>
		public static bool IsReadVertices(Version version)
		{
			return version.IsLess(5, 6);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadBindpose(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// 2018.1
		/// </summary>
		public static bool IsReadSourceSkin(Version version)
		{
			return version.IsEqual(2018, 1);
		}
		/// <summary>
		/// 5.4.5p1 to 5.5.0 exclusive or 5.5.0p3 or 5.5.3 and greater
		/// </summary>
		public bool IsReadAtlasRectOffset(Version version)
		{
			return (version.IsGreaterEqual(5, 4, 5, VersionType.Patch, 1) && version.IsLess(5, 5)) ||
				version.IsEqual(5, 5, 0, VersionType.Patch, 3) || version.IsGreaterEqual(5, 5, 3);
		}
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool IsReadUVTransform(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadDownscaleMultiplier(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		
		public Vector2f[][] GenerateOutline(Version version)
		{
			if(IsReadVertices(version))
			{
				Vector2f[][] outline = new Vector2f[1][];
				outline[0] = new Vector2f[Vertices.Count];
				for (int i = 0; i < Vertices.Count; i++)
				{
					outline[0][i] = Vertices[i].Position.ToVector2();
				}
				return outline;
			}
			else
			{
				List<Vector2f[]> outlines = new List<Vector2f[]>();
				foreach(SubMesh submesh in SubMeshes)
				{
					Vector3f[] vertices = VertexData.GenerateVertices(version, submesh);
					VerticesToOutline(outlines, vertices, submesh);
				}
				return outlines.ToArray();
			}
		}

		public void Read(AssetStream stream)
		{
			Texture.Read(stream);
			if (IsReadAlphaTexture(stream.Version))
			{
				AlphaTexture.Read(stream);
			}

			if (IsReadVertices(stream.Version))
			{
				m_vertices = stream.ReadArray<SpriteVertex>();
				m_indices = stream.ReadUInt16Array();
				stream.AlignStream(AlignType.Align4);
			}
			else
			{
				m_subMeshes = stream.ReadArray<SubMesh>();
				m_indexBuffer = stream.ReadByteArray();
				stream.AlignStream(AlignType.Align4);

				VertexData.Read(stream);
			}
			if (IsReadBindpose(stream.Version))
			{
				m_bindpose = stream.ReadArray<Matrix4x4f>();
			}
			if(IsReadSourceSkin(stream.Version))
			{
				m_sourceSkin = stream.ReadArray<BoneWeights4>();
			}

			TextureRect.Read(stream);
			TextureRectOffset.Read(stream);
			if(IsReadAtlasRectOffset(stream.Version))
			{
				AtlasRectOffset.Read(stream);
			}
			SettingsRaw = stream.ReadUInt32();
			if(IsReadUVTransform(stream.Version))
			{
				UVTransform.Read(stream);
			}
			if(IsReadDownscaleMultiplier(stream.Version))
			{
				DownscaleMultiplier = stream.ReadSingle();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Texture.FetchDependency(file, isLog, () => nameof(SpriteRenderData), "Texture");
			yield return AlphaTexture.FetchDependency(file, isLog, () => nameof(SpriteRenderData), "AlphaTexture");
		}

		private void VerticesToOutline(List<Vector2f[]> outlines, Vector3f[] vertexValues, SubMesh submesh)
		{
			int vertexCount = submesh.IndexCount / 3;
			List<Vector3i> vertices = new List<Vector3i>(vertexCount);
			using (MemoryStream memStream = new MemoryStream(m_indexBuffer))
			{
				using (BinaryReader reader = new BinaryReader(memStream))
				{
					memStream.Position = submesh.FirstByte;
					for (int i = 0; i < vertexCount; i++)
					{
						int x = reader.ReadInt16();
						int y = reader.ReadInt16();
						int z = reader.ReadInt16();
						vertices.Add(new Vector3i(x, y, z));
					}
				}
			}
			
			while (true)
			{
				Vector2f[] outline = VerticesToOutline(vertexValues, vertices);
				outlines.Add(outline);
				if (vertices.Count == 0)
				{
					break;
				}
			}
		}

		private static Vector2f[] VerticesToOutline(IReadOnlyList<Vector3f> vertexValues, List<Vector3i> vertices)
		{
			FindOutsideVertex(vertices, out int outsideVertexIndex, out int member);
			Vector3i outsideVertex = vertices[outsideVertexIndex];
			List<int> line = new List<int>();
			int point1 = outsideVertex.GetValueByMember(member);
			int point2 = outsideVertex.GetValueByMember(member + 1);
			line.Add(point1);
			line.Add(point2);

			int currentPoint = point2;
			int currentIndex = outsideVertexIndex;
			bool isMoreMeshes = false;
			Vector2f prevPointValue = vertexValues[point1].ToVector2();
			Vector2f curPointValue = vertexValues[point2].ToVector2();
			
			while (true)
			{
				int nextPoint = -1;
				int nextVertexIndex = -1;
				float lowestAngle = 360.0f;
				for (int i = 0; i < vertices.Count; i++)
				{
					if (i == currentIndex)
					{
						continue;
					}

					Vector3i vertex = vertices[i];
					if (!vertex.ContainsValue(currentPoint))
					{
						continue;
					}

					int checkPointMember = vertex.GetMemberByValue(currentPoint);
					int checkPoint = vertex.GetValueByMember(checkPointMember + 1);
					if (line.Contains(checkPoint))
					{
						continue;
					}

					Vector2f nextPointValue = vertexValues[checkPoint].ToVector2();
					float angle = Vector2f.AngleFrom3Points(prevPointValue, curPointValue, nextPointValue);
					if (angle < lowestAngle)
					{
						nextPoint = checkPoint;
						nextVertexIndex = i;
						lowestAngle = angle;
					}
				}

				if(nextVertexIndex == -1)
				{
					Vector3i currentVertex = vertices[currentIndex];
					int nextPointMember = currentVertex.GetMemberByValue(currentPoint);
					nextPoint = currentVertex.GetValueByMember(nextPointMember + 1);
					nextVertexIndex = currentIndex;
					if (line.Contains(nextPoint))
					{
						break;
					}
				}

				prevPointValue = curPointValue;
				curPointValue = vertexValues[nextPoint].ToVector2();

				currentPoint = nextPoint;
				line.Add(nextPoint);

				currentIndex = nextVertexIndex;
			}

			Vector2f[] outline = new Vector2f[line.Count];
			for (int i = 0; i < line.Count; i++)
			{
				outline[i] = vertexValues[line[i]].ToVector2();
			}
			
			HashSet<int> deletePoints = new HashSet<int>();
			foreach(int point in line)
			{
				deletePoints.Add(point);
			}
			while (true)
			{
				bool isNew = false;
				for(int i = 0; i < vertices.Count; i++)
				{
					Vector3i vertex = vertices[i];
					if (deletePoints.Contains(vertex.X))
					{
						isNew |= deletePoints.Add(vertex.Y);
						isNew |= deletePoints.Add(vertex.Z);
						vertices.RemoveAt(i--);
					}
					else if (deletePoints.Contains(vertex.Y))
					{
						isNew |= deletePoints.Add(vertex.Z);
						isNew |= deletePoints.Add(vertex.X);
						vertices.RemoveAt(i--);
					}
					else if (deletePoints.Contains(vertex.Z))
					{
						isNew |= deletePoints.Add(vertex.X);
						isNew |= deletePoints.Add(vertex.Y);
						vertices.RemoveAt(i--);
					}
				}
				if(!isNew)
				{
					break;
				}
			}

			return outline;
		}

		private static void FindOutsideVertex(IReadOnlyList<Vector3i> vertices, out int outsideVertexIndex, out int member)
		{
			for (outsideVertexIndex = 0; outsideVertexIndex < vertices.Count; outsideVertexIndex++)
			{
				Vector3i vertex = vertices[outsideVertexIndex];
				int xCount = 0;
				int yCount = 0;
				int zCount = 0;
				for (int j = 0; j < vertices.Count; j++)
				{
					Vector3i next = vertices[j];
					if (next.ContainsValue(vertex.X))
					{
						xCount++;
					}
					if (next.ContainsValue(vertex.Y))
					{
						yCount++;
					}
					if (next.ContainsValue(vertex.Z))
					{
						zCount++;
					}
					if (xCount >= 3 && yCount >= 3 && yCount >= 3)
					{
						break;
					}
				}

				if (xCount < 3)
				{
					member = yCount <= zCount ? 0 : 2;
					return;
				}
				if (yCount < 3)
				{
					member = zCount <= xCount ? 1 : 0;
					return;
				}
				if (zCount < 3)
				{
					member = xCount <= yCount ? 2 : 1;
					return;
				}
			}
			throw new Exception("Outside index wasn't found");
		}

		public IReadOnlyList<SpriteVertex> Vertices => m_vertices;
		public IReadOnlyList<ushort> Indices => m_indices;
		public IReadOnlyList<SubMesh> SubMeshes => m_subMeshes;
		public IReadOnlyList<byte> IndexBuffer => m_indexBuffer;
		public IReadOnlyList<Matrix4x4f> Bindpose => m_bindpose;
		public IReadOnlyList<BoneWeights4> SourceSkin => m_sourceSkin;
		public uint SettingsRaw { get; private set; }
		public float DownscaleMultiplier { get; private set; }

		public PPtr<Texture2D> Texture;
		public PPtr<Texture2D> AlphaTexture;
		public VertexData VertexData;
		public Rectf TextureRect;
		public Vector2f TextureRectOffset;
		public Vector2f AtlasRectOffset;
		public Vector4f UVTransform;

		private SpriteVertex[] m_vertices;
		private ushort[] m_indices;
		private SubMesh[] m_subMeshes;
		private byte[] m_indexBuffer;
		private Matrix4x4f[] m_bindpose;
		private BoneWeights4[] m_sourceSkin;
	}
}
