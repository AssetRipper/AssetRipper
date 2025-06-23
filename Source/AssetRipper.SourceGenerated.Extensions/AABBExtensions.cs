using AssetRipper.SourceGenerated.Subclasses.AABB;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AABBExtensions
{
	public static void CopyValuesFrom(this IAABB instance, Vector3 center, Vector3 extent)
	{
		instance.Center.CopyValues(center);
		instance.Extent.CopyValues(extent);
	}

	public static void Reset(this IAABB instance)
	{
		instance.Center.Reset();
		instance.Extent.Reset();
	}

	public static void CalculateFromVertexArray(this IAABB instance, ReadOnlySpan<Vector3> vertices)
	{
		if (vertices.Length == 0)
		{
			instance.Reset();
		}
		else
		{
			Vector3 first = vertices[0];
			float minX = first.X;
			float minY = first.Y;
			float minZ = first.Z;
			float maxX = first.X;
			float maxY = first.Y;
			float maxZ = first.Z;
			for (int i = 1; i < vertices.Length; i++)
			{
				Vector3 vertex = vertices[i];
				if (vertex.X < minX)
				{
					minX = vertex.X;
				}
				else if (vertex.X > maxX)
				{
					maxX = vertex.X;
				}

				if (vertex.Y < minY)
				{
					minY = vertex.Y;
				}
				else if (vertex.Y > maxY)
				{
					maxY = vertex.Y;
				}

				if (vertex.Z < minZ)
				{
					minZ = vertex.Z;
				}
				else if (vertex.Z > maxZ)
				{
					maxZ = vertex.Z;
				}
			}
			Vector3 center = new Vector3((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2);
			Vector3 extent = new Vector3((maxX - minX) / 2, (maxY - minY) / 2, (maxZ - minZ) / 2);
			instance.CopyValuesFrom(center, extent);
		}
	}
}
