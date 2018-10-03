using System;
using System.Collections.Generic;

namespace uTinyRipper.Classes.Sprites.Utils
{
	public class MeshOutlineGenerator
	{
		private struct OutsideInfo
		{
			public OutsideInfo(int triIndex, int vertex)
			{
				Triangle = triIndex;
				OutsideMember = vertex;
			}

			public override string ToString()
			{
				return $"{Triangle}:{OutsideMember}";
			}

			public int Triangle { get; }
			public int OutsideMember { get; }
		}

		public MeshOutlineGenerator(IReadOnlyList<Vector3f> vertices, IReadOnlyList<Vector3i> triangles)
		{
			if(vertices == null)
			{
				throw new ArgumentNullException(nameof(vertices));
			}
			if (triangles == null)
			{
				throw new ArgumentNullException(nameof(triangles));
			}
			m_vertices = vertices;
			m_triangles.AddRange(triangles);
		}

		public List<Vector2f[]> GenerateOutlines()
		{
			List<Vector2f[]> outlines = new List<Vector2f[]>();
			GenerateOutsideTriangles();
			while (m_outside.Count > 0)
			{
				Vector2f[] outline = GenerateOutline();
				outlines.Add(outline);
			}
			return outlines;
		}
		
		private Vector2f[] GenerateOutline()
		{
			List<Vector2f> outline = new List<Vector2f>();
			OutsideInfo info = m_outside[0];
			Vector3i tri = m_triangles[info.Triangle];
			int first = tri.GetValueByMember(info.OutsideMember);
			int second = tri.GetValueByMember(info.OutsideMember + 1);
			outline.Add(m_vertices[first].ToVector2());
			outline.Add(m_vertices[second].ToVector2());

			Vector3i lastTri = tri;
			int lastMember = info.OutsideMember + 1;
			int lastVertex = lastTri.GetValueByMember(info.OutsideMember);
			while (true)
			{
				int vertex = lastTri.GetValueByMember(lastMember);
				if (GetInfoByVertex(vertex, out info))
				{
					RemoveOutsideInfo(lastVertex);

					lastTri = m_triangles[info.Triangle];
					lastMember = info.OutsideMember + 1;
					lastVertex = lastTri.GetValueByMember(info.OutsideMember);
				}
				else
				{
					lastMember++;
					if(lastTri.GetValueByMember(lastMember) == lastVertex)
					{
						break;
					}
				}

				int nextVertex = lastTri.GetValueByMember(lastMember);
				if (nextVertex == first)
				{
					break;
				}

				outline.Add(m_vertices[nextVertex].ToVector2());
			}
			RemoveOutsideInfo(lastVertex);

			return outline.ToArray();
		}

		private void GenerateOutsideTriangles()
		{
			m_outside.Clear();
			for (int i = 0; i < m_triangles.Count; i++)
			{
				if (GetOutsideTriangle(i, out OutsideInfo info))
				{
					m_outside.Add(info);
				}
			}
		}

		private bool GetOutsideTriangle(int index, out OutsideInfo info)
		{
			Vector3i check = m_triangles[index];
			int neighborCount = 0;
			bool xy = true;
			bool yz = true;
			bool zx = true;
			info = default;
			for (int i = 0; i < m_triangles.Count; i++)
			{
				if(i == index)
				{
					continue;
				}
				Vector3i triangle = m_triangles[i];
				if (triangle.ContainsValue(check.X))
				{
					if (triangle.ContainsValue(check.Y))
					{
						xy = false;
						if(++neighborCount == 3)
						{
							return false;
						}
					}
					else if(triangle.ContainsValue(check.Z))
					{
						zx = false;
						if (++neighborCount == 3)
						{
							return false;
						}
					}
				}
				else if (triangle.ContainsValue(check.Y) && (triangle.ContainsValue(check.Z)))
				{
					yz = false;
					if (++neighborCount == 3)
					{
						return false;
					}
				}
			}
			int vertex = xy ? (zx ? 2 : 0) : (yz ? 1 : 2);
			info = new OutsideInfo(index, vertex);
			return true;
		}

		private bool GetInfoByVertex(int vertex, out OutsideInfo result)
		{
			foreach (OutsideInfo info in m_outside)
			{
				Vector3i triangle = m_triangles[info.Triangle];
				if (triangle.GetValueByMember(info.OutsideMember) == vertex)
				{
					result = info;
					return true;
				}
			}
			result = default;
			return false;
		}

		private void RemoveOutsideInfo(int outVertex)
		{
			for(int i = 0; i < m_outside.Count; i++)
			{
				OutsideInfo info = m_outside[i];
				if (m_triangles[info.Triangle].GetValueByMember(info.OutsideMember) == outVertex)
				{
					m_outside.RemoveAt(i);
					return;
				}
			}
		}

		private readonly IReadOnlyList<Vector3f> m_vertices;
		private readonly List<Vector3i> m_triangles = new List<Vector3i>();
		private readonly List<OutsideInfo> m_outside = new List<OutsideInfo>();
	}
}
