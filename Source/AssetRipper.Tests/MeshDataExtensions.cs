using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using System.Numerics;

namespace AssetRipper.Tests;

internal static class MeshDataExtensions
{
	extension(MeshData)
	{
		public static MeshData CreateTriangleMesh()
		{
			Vector3[] vertices =
			[
				new Vector3(0.0f, 0.0f, 0.0f),
				new Vector3(1.0f, 0.0f, 0.0f),
				new Vector3(0.0f, 1.0f, 0.0f),
			];
			Vector3[] normals =
			[
				new Vector3(0.0f, 0.0f, 1.0f),
				new Vector3(0.0f, 0.0f, 1.0f),
				new Vector3(0.0f, 0.0f, 1.0f),
			];
			Vector4[] tangents =
			[
				new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
			];
			ColorFloat[] colors =
			[
				new ColorFloat(1.0f, 0.0f, 0.0f, 1.0f),
				new ColorFloat(0.0f, 1.0f, 0.0f, 1.0f),
				new ColorFloat(0.0f, 0.0f, 1.0f, 1.0f),
			];
			ReadOnlySpan<Vector2> uv =
			[
				new Vector2(0.0f, 0.0f),
				new Vector2(1.0f, 0.0f),
				new Vector2(0.0f, 1.0f),
			];
			uint[] processedIndexBuffer = [0, 1, 2];
			SubMeshData[] subMeshes =
			[
				new SubMeshData(0, 0, 0, processedIndexBuffer.Length, 1, 3, MeshTopology.Triangles, new Bounds(new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f)))
			];
			return new MeshData(vertices, normals, tangents, colors, uv.ToArray(), uv.ToArray(), uv.ToArray(), uv.ToArray(), uv.ToArray(), uv.ToArray(), uv.ToArray(), uv.ToArray(), null, null, processedIndexBuffer, subMeshes);
		}
	}
}
