using AssetRipper.IO.Endian;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions;
using System.Collections;
using System.Runtime.CompilerServices;

namespace AssetRipper.Tests;

internal class VertexDataBlobTests
{
	[Theory]
	public void TriangleMeshIsPreserved([Values("3.5", "4", "5", "2017", "2018", "2018.4")] string versionString, EndianType endianType)
	{
		MeshData meshData = MeshData.CreateTriangleMesh();
		UnityVersion version = UnityVersion.Parse(versionString);
		VertexDataBlob vertexDataBlob = VertexDataBlob.Create(meshData, version, endianType);
		MeshData meshData2 = vertexDataBlob.ToMeshData();

		using (Assert.EnterMultipleScope())
		{
			AssertEquivalence(meshData2.Vertices, meshData.Vertices);
			AssertEquivalence(meshData2.Normals, meshData.Normals);
			AssertEquivalence(meshData2.Tangents, meshData.Tangents);
			AssertEquivalence(meshData2.Colors, meshData.Colors);
			AssertEquivalence(meshData2.UV0, meshData.UV0);
			AssertEquivalence(meshData2.UV1, meshData.UV1);

			if (VertexDataBlob.SupportsUV2(version))
			{
				AssertEquivalence(meshData2.UV2, meshData.UV2);
			}
			else
			{
				Assert.That(meshData2.UV2, Is.Null);
			}

			if (VertexDataBlob.SupportsUV3(version))
			{
				AssertEquivalence(meshData2.UV3, meshData.UV3);
			}
			else
			{
				Assert.That(meshData2.UV3, Is.Null);
			}

			if (VertexDataBlob.SupportsUV4(version))
			{
				AssertEquivalence(meshData2.UV4, meshData.UV4);
			}
			else
			{
				Assert.That(meshData2.UV4, Is.Null);
			}

			if (VertexDataBlob.SupportsUV5(version))
			{
				AssertEquivalence(meshData2.UV5, meshData.UV5);
			}
			else
			{
				Assert.That(meshData2.UV5, Is.Null);
			}

			if (VertexDataBlob.SupportsUV6(version))
			{
				AssertEquivalence(meshData2.UV6, meshData.UV6);
			}
			else
			{
				Assert.That(meshData2.UV6, Is.Null);
			}

			if (VertexDataBlob.SupportsUV7(version))
			{
				AssertEquivalence(meshData2.UV7, meshData.UV7);
			}
			else
			{
				Assert.That(meshData2.UV7, Is.Null);
			}

			if (VertexDataBlob.SupportsSkin(version))
			{
				AssertEquivalence(meshData2.Skin, meshData.Skin);
			}
			else
			{
				Assert.That(meshData2.Skin, Is.Null);
			}
		}
	}

	private static void AssertEquivalence(
		IEnumerable? actual,
		IEnumerable? expected,
		[CallerArgumentExpression(nameof(expected))] string constraintExpression = "",
		[CallerArgumentExpression(nameof(actual))] string actualExpression = "")
	{
		if (expected is null)
		{
			Assert.That(actual, Is.Null, actualExpression: actualExpression);
		}
		else
		{
			Assert.That(actual, Is.Not.Null, actualExpression: actualExpression);
			Assert.That(actual, Is.EquivalentTo(expected), actualExpression: actualExpression, constraintExpression: constraintExpression);
		}
	}
}
