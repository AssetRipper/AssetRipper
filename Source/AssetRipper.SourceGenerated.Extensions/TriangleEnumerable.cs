using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using System.Collections;

namespace AssetRipper.SourceGenerated.Extensions;

public readonly struct TriangleEnumerable : IEnumerable<(uint, uint, uint)>
{
	private readonly MeshTopology topology;
	private readonly ArraySegment<uint> indexBuffer;

	public TriangleEnumerable(ISubMesh subMesh, IndexFormat indexFormat, uint[] indexBuffer) : this(subMesh.GetTopology(), GetIndexBufferSegment(subMesh, indexFormat, indexBuffer))
	{
	}

	public TriangleEnumerable(MeshTopology topology, ArraySegment<uint> indexBuffer)
	{
		ThrowIfNotSupported(topology);
		this.topology = topology;
		this.indexBuffer = indexBuffer;
	}

	public static bool IsSupported(MeshTopology topology)
	{
		return topology is MeshTopology.Triangles or MeshTopology.TriangleStrip or MeshTopology.Quads;
	}

	public IEnumerator<(uint, uint, uint)> GetEnumerator()
	{
		switch (topology)
		{
			case MeshTopology.Triangles:
				{
					for (int i = 0; i < indexBuffer.Count; i += 3)
					{
						yield return (indexBuffer[i], indexBuffer[i + 1], indexBuffer[i + 2]);
					}
				}
				break;
			case MeshTopology.TriangleStrip:
				{
					// de-stripify :
					for (int i = 0; i < indexBuffer.Count - 2; i++)
					{
						uint a = indexBuffer[i];
						uint b = indexBuffer[i + 1];
						uint c = indexBuffer[i + 2];

						// skip degenerates
						if (a == b || a == c || b == c)
						{
							continue;
						}

						// do the winding flip-flop of strips :
						if ((i & 1) == 1)
						{
							yield return (b, a, c);
						}
						else
						{
							yield return (a, b, c);
						}
					}
				}
				break;
			case MeshTopology.Quads:
				{
					for (int q = 0; q < indexBuffer.Count; q += 4)
					{
						yield return(indexBuffer[q], indexBuffer[q + 1], indexBuffer[q + 2]);
						yield return(indexBuffer[q], indexBuffer[q + 2], indexBuffer[q + 3]);
					}
				}
				break;
		}
	}

	private static ArraySegment<uint> GetIndexBufferSegment(ISubMesh subMesh, IndexFormat indexFormat, uint[] indexBuffer)
	{
		if (indexFormat == IndexFormat.UInt16)
		{
			return new ArraySegment<uint>(indexBuffer, (int)subMesh.FirstByte / sizeof(ushort), (int)subMesh.IndexCount);
		}
		else
		{
			return new ArraySegment<uint>(indexBuffer, (int)subMesh.FirstByte / sizeof(uint), (int)subMesh.IndexCount);
		}
	}

	private static void ThrowIfNotSupported(MeshTopology topology)
	{
		if (!IsSupported(topology))
		{
			throw new ArgumentException($"Mesh topology {topology} is not supported", nameof(topology));
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
