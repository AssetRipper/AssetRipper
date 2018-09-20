using System;
using System.Collections.Generic;

namespace uTinyRipper.Classes.Sprites.Utils
{
	public class MeshOutlineGenerator
	{
		private class OutsideVertexSearcher
		{
			public OutsideVertexSearcher(IReadOnlyList<Vector3f> vertices, IReadOnlyList<Vector3i> triangles, List<int> line)
			{
				if (vertices == null)
				{
					throw new ArgumentNullException(nameof(vertices));
				}
				if (triangles == null)
				{
					throw new ArgumentNullException(nameof(triangles));
				}
				if (line == null)
				{
					throw new ArgumentNullException(nameof(line));
				}
				m_vertices = vertices;
				m_triangles = triangles;
				m_line = line;
			}

			private OutsideVertexSearcher(OutsideVertexSearcher copy):
				this(copy.m_vertices, copy.m_triangles, copy.m_line)
			{
				CurrentTriangle = copy.CurrentTriangle;
				PreviousVertex = copy.PreviousVertex;
				CurrentVertex = copy.CurrentVertex;
			}

			public void SearchNext()
			{
				NextVertex = -1;
				NextTriangle = -1;
				float lowestAngle = 360.0f;
				for (int i = 0; i < m_triangles.Count; i++)
				{
					if (i == CurrentTriangle)
					{
						continue;
					}

					Vector3i triangle = m_triangles[i];
					if (!triangle.ContainsValue(CurrentVertex))
					{
						continue;
					}

					int checkVertexMember = triangle.GetMemberByValue(CurrentVertex);
					int checkVertex = triangle.GetValueByMember(checkVertexMember + 1);
					if (m_line.Contains(checkVertex))
					{
						continue;
					}

					Vector2f checkVertexValue = m_vertices[checkVertex].ToVector2();
					int checkTriangleIndex = i;
					if (checkVertexValue == CurrentVertexValue)
					{
						// recursivly find next non matching vertex
						m_line.Add(checkVertex);
						OutsideVertexSearcher deepSearcher = new OutsideVertexSearcher(this);
						deepSearcher.CurrentTriangle = i;
						deepSearcher.CurrentVertex = checkVertex;
						deepSearcher.SearchNext();
						m_line.RemoveAt(m_line.Count - 1);

						if (deepSearcher.NextVertex == -1)
						{
							continue;
						}

						checkVertex = deepSearcher.NextVertex;
						checkVertexValue = m_vertices[deepSearcher.NextVertex].ToVector2();
						checkTriangleIndex = deepSearcher.NextTriangle;
					}

					float angle = Vector2f.AngleFrom3Points(PreviousVertexValue, CurrentVertexValue, checkVertexValue);
					if (angle <= lowestAngle)
					{
						NextVertex = checkVertex;
						NextTriangle = checkTriangleIndex;
						lowestAngle = angle;
					}
				}
			}

			public int CurrentTriangle { get; set; }
			public int PreviousVertex { get; set; }
			public int CurrentVertex { get; set; }
			public int NextVertex { get; private set; }
			public int NextTriangle { get; private set; }

			private Vector2f PreviousVertexValue => m_vertices[PreviousVertex].ToVector2();
			private Vector2f CurrentVertexValue => m_vertices[CurrentVertex].ToVector2();

			private readonly IReadOnlyList<Vector3f> m_vertices;
			private readonly IReadOnlyList<Vector3i> m_triangles;
			private readonly List<int> m_line;
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
			while (m_triangles.Count > 0)
			{
				Vector2f[] outline = GenerateOutline();
				outlines.Add(outline);
			}
			return outlines;
		}


		private Vector2f[] GenerateOutline()
		{
			FindOutsideTriangle(out int outsideTriangleIndex, out int outsideMember);
			Vector3i outsideTriangle = m_triangles[outsideTriangleIndex];
			List<int> line = new List<int>();
			int vertex1 = outsideTriangle.GetValueByMember(outsideMember);
			int vertex2 = outsideTriangle.GetValueByMember(outsideMember + 1);
			line.Add(vertex1);
			line.Add(vertex2);
			
			OutsideVertexSearcher searcher = new OutsideVertexSearcher(m_vertices, m_triangles, line);
			searcher.CurrentTriangle = outsideTriangleIndex;
			searcher.PreviousVertex = vertex1;
			searcher.CurrentVertex = vertex2;
			while (true)
			{
				searcher.SearchNext();

				if (searcher.NextVertex == -1)
				{
					Vector3i currentTriangle = m_triangles[searcher.CurrentTriangle];
					int nextPointMember = currentTriangle.GetMemberByValue(searcher.CurrentVertex);
					int nextVertex = currentTriangle.GetValueByMember(nextPointMember + 1);
					searcher.PreviousVertex = searcher.CurrentVertex;
					searcher.CurrentVertex = nextVertex;
					if (line.Contains(nextVertex))
					{
						break;
					}
				}
				else
				{
					searcher.CurrentTriangle = searcher.NextTriangle;
					searcher.PreviousVertex = searcher.CurrentVertex;
					searcher.CurrentVertex = searcher.NextVertex;
				}
				
				line.Add(searcher.CurrentVertex);
			}

			Vector2f[] outline = new Vector2f[line.Count];
			for (int i = 0; i < line.Count; i++)
			{
				outline[i] = m_vertices[line[i]].ToVector2();
			}

			HashSet<int> deletePoints = new HashSet<int>();
			foreach (int point in line)
			{
				deletePoints.Add(point);
			}
			while (true)
			{
				bool isNew = false;
				for (int i = 0; i < m_triangles.Count; i++)
				{
					Vector3i vertex = m_triangles[i];
					if (deletePoints.Contains(vertex.X))
					{
						isNew |= deletePoints.Add(vertex.Y);
						isNew |= deletePoints.Add(vertex.Z);
						m_triangles.RemoveAt(i--);
					}
					else if (deletePoints.Contains(vertex.Y))
					{
						isNew |= deletePoints.Add(vertex.Z);
						isNew |= deletePoints.Add(vertex.X);
						m_triangles.RemoveAt(i--);
					}
					else if (deletePoints.Contains(vertex.Z))
					{
						isNew |= deletePoints.Add(vertex.X);
						isNew |= deletePoints.Add(vertex.Y);
						m_triangles.RemoveAt(i--);
					}
				}
				if (!isNew)
				{
					break;
				}
			}

			return outline;
		}
		
		private void FindOutsideTriangle(out int outsideTriangle, out int outsideMember)
		{
			for (outsideTriangle = 0; outsideTriangle < m_triangles.Count; outsideTriangle++)
			{
				Vector3i vertex = m_triangles[outsideTriangle];
				int xCount = 0;
				int yCount = 0;
				int zCount = 0;
				for (int j = 0; j < m_triangles.Count; j++)
				{
					Vector3i next = m_triangles[j];
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
					if (xCount >= 3 && yCount >= 3 && zCount >= 3)
					{
						break;
					}
				}

				if (xCount < 3)
				{
					outsideMember = yCount <= zCount ? 0 : 2;
					return;
				}
				if (yCount < 3)
				{
					outsideMember = zCount <= xCount ? 1 : 0;
					return;
				}
				if (zCount < 3)
				{
					outsideMember = xCount <= yCount ? 2 : 1;
					return;
				}
			}
			throw new Exception("Outside triangle hasn't been found");
		}

		private readonly IReadOnlyList<Vector3f> m_vertices;
		private readonly List<Vector3i> m_triangles = new List<Vector3i>();
	}
}
