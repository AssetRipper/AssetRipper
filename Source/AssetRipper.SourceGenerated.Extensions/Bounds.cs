using AssetRipper.SourceGenerated.Subclasses.AABB;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public record struct Bounds(
	Vector3 Center,
	Vector3 Extent)
{
	public static implicit operator Bounds(AABB aabb)
	{
		return new Bounds(aabb.Center, aabb.Extent);
	}

	public readonly void CopyTo(AABB destination)
	{
		destination.Center.CopyValues(Center);
		destination.Extent.CopyValues(Extent);
	}

	public static Bounds CalculateFromVertexArray(ReadOnlySpan<Vector3> vertices)
	{
		if (vertices.Length == 0)
		{
			return default;
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
			return new(center, extent);
		}
	}
}
