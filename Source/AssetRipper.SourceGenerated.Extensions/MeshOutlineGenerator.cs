using AssetRipper.Numerics;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public sealed class MeshOutlineGenerator
{
	private readonly struct Outline
	{
		private readonly record struct Outside(int Triangle, int Member);

		public Outline(IReadOnlyList<Vector3i> triangles, int startTriangle)
		{
			m_triangles = triangles ?? throw new ArgumentNullException(nameof(triangles));

			GenerateIndexes(startTriangle);
			GenerateOutsiders();
		}

		public void GenerateOutline()
		{
			m_outline.Clear();
			Outside outsider = m_outsiders[0];
			Vector3i tri = m_triangles[outsider.Triangle];
			int first = tri.GetValueByMember(outsider.Member);
			int second = tri.GetValueByMember(outsider.Member + 1);
			int startTriIndex = outsider.Triangle;
			m_outline.Add(first);
			m_outline.Add(second);

			Vector3i lastTri = tri;
			int lastMember = outsider.Member + 1;
			int lastVertex = lastTri.GetValueByMember(outsider.Member);
			int lastTriIndex = outsider.Triangle;
			while (true)
			{
				if (GetNextOutsider(lastTri, lastMember, out outsider))
				{
					lastTri = m_triangles[outsider.Triangle];
					lastMember = outsider.Member + 1;
					lastVertex = lastTri.GetValueByMember(outsider.Member);
					lastTriIndex = outsider.Triangle;
				}
				else
				{
					lastMember++;
					if (lastTri.GetValueByMember(lastMember) == lastVertex)
					{
						if (lastVertex != first)
						{
							break;
						}
					}
				}

				int nextVertex = lastTri.GetValueByMember(lastMember);
				if (nextVertex == first)
				{
					break;
				}

				m_outline.Add(nextVertex);
			}
		}

		public bool Contains(int triIndex) => m_indexes.Contains(triIndex);

		private int GetEdgeMask(Vector3i triangle, IReadOnlyList<int> indexes)
		{
			int edgeMask = 0;
			for (int i = 0; i < indexes.Count; i++)
			{
				Vector3i check = m_triangles[indexes[i]];
				if (check == triangle)
				{
					continue;
				}
				int edge = GetNeighborEdge(triangle, check);
				if (edge == 0)
				{
					continue;
				}
				edgeMask |= edge;
			}
			return edgeMask;
		}

		private static int GetNeighborEdge(Vector3i tri1, Vector3i tri2)
		{
			if (IsNeighbors(tri1.X, tri1.Y, tri2))
			{
				return 1 << 0;
			}
			if (IsNeighbors(tri1.Y, tri1.Z, tri2))
			{
				return 1 << 1;
			}
			if (IsNeighbors(tri1.Z, tri1.X, tri2))
			{
				return 1 << 2;
			}
			return 0;
		}

		private static bool IsNeighbors(int a, int b, Vector3i tri2)
		{
			if (a == tri2.X && b == tri2.Z)
			{
				return true;
			}
			if (b == tri2.Y && a == tri2.Z)
			{
				return true;
			}
			if (b == tri2.X && a == tri2.Y)
			{
				return true;
			}
			return false;
		}

		private void GenerateIndexes(int startTriangle)
		{
			List<int> indexes = new List<int>();
			indexes.Add(startTriangle);
			m_indexes.Add(startTriangle);
			for (int i = 0; i < indexes.Count; i++)
			{
				int index = indexes[i];
				Vector3i triangle = m_triangles[index];
				//int edgeMask = GetEdgeMask(triangle, indexes);
				for (int j = 0; j < m_triangles.Count; j++)
				{
					if (m_indexes.Contains(j))
					{
						continue;
					}
					Vector3i check = m_triangles[j];
					int edge = GetNeighborEdge(triangle, check);
					if (edge == 0)
					{
						continue;
					}
					// allow only one neighbor per edge
					//if ((edgeMask & edge) != 0)
					{
						//continue;
					}

					indexes.Add(j);
					m_indexes.Add(j);
					//edgeMask |= edge;
				}
			}
		}

		private void GenerateOutsiders()
		{
			foreach (int index in m_indexes)
			{
				if (GenerateOutsider(index, out Outside outsider))
				{
					m_outsiders.Add(outsider);
				}
			}
		}

		private bool GenerateOutsider(int index, out Outside outsider)
		{
			Vector3i triangle = m_triangles[index];
			int neighborCount = 0;
			bool xy = true;
			bool yz = true;
			bool zx = true;
			outsider = new();
			foreach (int checkIndex in m_indexes)
			{
				if (checkIndex == index)
				{
					continue;
				}
				Vector3i check = m_triangles[checkIndex];
				if (check.ContainsValue(triangle.X))
				{
					if (check.ContainsValue(triangle.Y))
					{
						xy = false;
						if (++neighborCount == 3)
						{
							return false;
						}
					}
					else if (check.ContainsValue(triangle.Z))
					{
						zx = false;
						if (++neighborCount == 3)
						{
							return false;
						}
					}
				}
				else if (check.ContainsValue(triangle.Y) && check.ContainsValue(triangle.Z))
				{
					yz = false;
					if (++neighborCount == 3)
					{
						return false;
					}
				}
			}
			int vertex = xy ? zx ? 2 : 0 : yz ? 1 : 2;
			outsider = new Outside(index, vertex);
			return true;
		}

		private bool GetNextOutsider(Vector3i triangle, int member, out Outside result)
		{
			int vertex = triangle.GetValueByMember(member);
			foreach (Outside outsider in m_outsiders)
			{
				Vector3i check = m_triangles[outsider.Triangle];
				if (check.GetValueByMember(outsider.Member) == vertex)
				{
					if (IsConnectedNeighbors(triangle, check, vertex))
					{
						result = outsider;
						return true;
					}
				}
			}
			result = default;
			return false;
		}

		private bool GetNextNeighbor(Vector3i tri, int vertex, out Vector3i result)
		{
			int member = tri.GetMemberByValue(vertex);
			int nextVertex = tri.GetValueByMember(member + 1);
			foreach (int index in m_indexes)
			{
				Vector3i check = m_triangles[index];
				if (check.ContainsValue(vertex) && check.ContainsValue(nextVertex))
				{
					if (check != tri)
					{
						result = check;
						return true;
					}
				}
			}
			result = default;
			return false;
		}

		private bool IsConnectedNeighbors(Vector3i tri1, Vector3i tri2, int vertex)
		{
			if (GetNeighborEdge(tri1, tri2) != 0)
			{
				return true;
			}

			Vector3i next = tri1;
			while (GetNextNeighbor(next, vertex, out next))
			{
				if (next == tri2)
				{
					return true;
				}
			}
			return false;
		}

		public int TriangleCount => m_indexes.Count;
		public IReadOnlyList<int> GeneratedOutline => m_outline;

		private readonly IReadOnlyList<Vector3i> m_triangles;
		private readonly HashSet<int> m_indexes = new();
		private readonly List<int> m_outline = new();
		private readonly List<Outside> m_outsiders = new();
	}

	public MeshOutlineGenerator(IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector3i> triangles)
	{
		ArgumentNullException.ThrowIfNull(vertices);
		ArgumentNullException.ThrowIfNull(triangles);
		m_vertices = vertices;
		foreach (Vector3i triangle in triangles)
		{
			if (IsValidTriangle(triangle))
			{
				m_triangles.Add(triangle);
			}
		}
	}

	public List<Vector2[]> GenerateOutlines()
	{
		List<Outline> outlines = new();
		for (int i = 0; i < m_triangles.Count; i++)
		{
			bool isKnown = false;
			for (int j = 0; j < outlines.Count; j++)
			{
				if (outlines[j].Contains(i))
				{
					isKnown = true;
					break;
				}
			}
			if (isKnown)
			{
				continue;
			}

			Outline outline = new Outline(m_triangles, i);
			outline.GenerateOutline();
			outlines.Add(outline);
		}

		List<Vector2[]> result = new();
		List<Vector2> resultLine = new();
		for (int i = 0; i < outlines.Count; i++)
		{
			resultLine.Clear();
			Outline outline = outlines[i];
			for (int j = 0; j < outline.GeneratedOutline.Count; j++)
			{
				int vertex = outline.GeneratedOutline[j];
				// commented out by mafaca
				// include outlines that has common vertex with current outline
				/*for (int k = i + 1; k < outlines.Count; k++)
				{
					Outline nextOutline = outlines[k];
					int index = nextOutline.GeneratedOutline.IndexOf(vertex);
					if (index != -1)
					{
						for (int l = index; l < nextOutline.GeneratedOutline.Count; l++)
						{
							int nextVertex = nextOutline.GeneratedOutline[l];
							resultLine.Add(m_vertices[nextVertex].AsVector2());
						}
						for (int m = 0; m < index; m++)
						{
							int nextVertex = nextOutline.GeneratedOutline[m];
							resultLine.Add(m_vertices[nextVertex].AsVector2());
						}
						outlines.RemoveAt(k--);
					}
				}*/
				resultLine.Add(m_vertices[vertex].AsVector2());
			}
			result.Add(resultLine.ToArray());
		}

		return result;
	}

	private static bool IsValidTriangle(Vector3i triangle)
	{
		return triangle.X != triangle.Y && triangle.X != triangle.Z && triangle.Y != triangle.Z;
	}

	private readonly IReadOnlyList<Vector3> m_vertices;
	private readonly List<Vector3i> m_triangles = new();
}
