﻿using AssetRipper.IO.Endian;
using AssetRipper.Numerics;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Extensions;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Tests;

internal class VertexDataBlobTests
{
	private static MeshData CreateTriangleMesh()
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
		Vector2[] uv =
		[
			new Vector2(0.0f, 0.0f),
			new Vector2(1.0f, 0.0f),
			new Vector2(0.0f, 1.0f),
		];
		uint[] processedIndexBuffer = [ 0, 1, 2 ];
		return new MeshData(vertices, normals, tangents, colors, uv, uv, uv, uv, uv, uv, uv, uv, null, processedIndexBuffer);
	}

	[Theory]
	public void TriangleMeshIsPreserved([Values("3.5", "4", "5", "2017", "2018", "2018.4")] string versionString, EndianType endianType)
	{
		MeshData meshData = CreateTriangleMesh();
		UnityVersion version = UnityVersion.Parse(versionString);
		VertexDataBlob vertexDataBlob = VertexDataBlob.Create(meshData, version, endianType);
		MeshData meshData2 = vertexDataBlob.ToMeshData();

		Assert.Multiple(() =>
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
		});
	}

	private static EquivalenceResolveConstraint IsEquivalentTo(IEnumerable? expected)
	{
		return new EquivalenceResolveConstraint(expected);
	}

	private sealed class EquivalenceResolveConstraint(IEnumerable? enumerable) : IResolveConstraint
	{
		public IConstraint Resolve()
		{
			return enumerable is null ? Is.Null : Is.Not.Null.And.EquivalentTo(enumerable);
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
		else if (actual is null)
		{
			Assert.That(actual, Is.Not.Null, actualExpression: actualExpression);
		}
		else
		{
#pragma warning disable NUnit2050 // NUnit 4 no longer supports string.Format specification
			Assert.That(actual, Is.EquivalentTo(expected), actualExpression: actualExpression, constraintExpression: constraintExpression);
#pragma warning restore NUnit2050 // NUnit 4 no longer supports string.Format specification
		}
	}
}
