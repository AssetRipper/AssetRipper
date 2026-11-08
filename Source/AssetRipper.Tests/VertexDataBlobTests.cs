using AssetRipper.IO.Endian;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions;
using System.Runtime.CompilerServices;

namespace AssetRipper.Tests;

internal class VertexDataBlobTests
{
	[Test]
	[MatrixDataSource]
	public async Task TriangleMeshIsPreserved([Matrix("3.5", "4", "5", "2017", "2018", "2018.2", "2018.4")] string versionString, EndianType endianType)
	{
		MeshData meshData = MeshData.CreateTriangleMesh();
		UnityVersion version = UnityVersion.Parse(versionString);
		VertexDataBlob vertexDataBlob = VertexDataBlob.Create(meshData, version, endianType);
		MeshData meshData2 = vertexDataBlob.ToMeshData();


		{
			await AssertEquivalence(meshData2.Vertices, meshData.Vertices);
			await AssertEquivalence(meshData2.Normals, meshData.Normals);
			await AssertEquivalence(meshData2.Tangents, meshData.Tangents);
			await AssertEquivalence(meshData2.Colors, meshData.Colors);
			await AssertEquivalence(meshData2.UV0, meshData.UV0);
			await AssertEquivalence(meshData2.UV1, meshData.UV1);

			if (VertexDataBlob.SupportsUV2(version))
			{
				await AssertEquivalence(meshData2.UV2, meshData.UV2);
			}
			else
			{
				await Assert.That(meshData2.UV2).IsNull();
			}

			if (VertexDataBlob.SupportsUV3(version))
			{
				await AssertEquivalence(meshData2.UV3, meshData.UV3);
			}
			else
			{
				await Assert.That(meshData2.UV3).IsNull();
			}

			if (VertexDataBlob.SupportsUV4(version))
			{
				await AssertEquivalence(meshData2.UV4, meshData.UV4);
			}
			else
			{
				await Assert.That(meshData2.UV4).IsNull();
			}

			if (VertexDataBlob.SupportsUV5(version))
			{
				await AssertEquivalence(meshData2.UV5, meshData.UV5);
			}
			else
			{
				await Assert.That(meshData2.UV5).IsNull();
			}

			if (VertexDataBlob.SupportsUV6(version))
			{
				await AssertEquivalence(meshData2.UV6, meshData.UV6);
			}
			else
			{
				await Assert.That(meshData2.UV6).IsNull();
			}

			if (VertexDataBlob.SupportsUV7(version))
			{
				await AssertEquivalence(meshData2.UV7, meshData.UV7);
			}
			else
			{
				await Assert.That(meshData2.UV7).IsNull();
			}

			if (VertexDataBlob.SupportsSkin(version))
			{
				await AssertEquivalence(meshData2.Skin, meshData.Skin);
			}
			else
			{
				await Assert.That(meshData2.Skin).IsNull();
			}
		}
	}

	private static async Task AssertEquivalence<T>(
		IEnumerable<T>? actual,
		IEnumerable<T>? expected,
		[CallerArgumentExpression(nameof(expected))] string constraintExpression = "",
		[CallerArgumentExpression(nameof(actual))] string actualExpression = "")
	{
		if (actual is null || expected is null)
		{
			await Assert.That(actual is null).IsEqualTo(expected is null)
				.Because($"{actualExpression} and {constraintExpression} should agree on nullness.");
			return;
		}

		await Assert.That(actual.SequenceEqual(expected)).IsTrue()
			.Because($"{actualExpression} should be equivalent to {constraintExpression}");
	}
}
