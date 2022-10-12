using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Subclasses.CompressedMesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Tests
{
	internal class CompressedMeshTests
	{
		private static readonly Random random = new Random(57089);
		private static readonly Vector3[] vectors = MakeUnitVectors(20);
		private static readonly uint[] integers = MakeUInts(24);

		private static Vector3[] MakeUnitVectors(int count)
		{
			Vector3[] result = new Vector3[count];
			for (int i = 0; i < count; i++)
			{
				float x = random.NextSingle();
				float y = random.NextSingle();
				float z = random.NextSingle();
				result[i] = Vector3.Normalize(new Vector3(x, y, z));
			}
			return result;
		}

		private static uint[] MakeUInts(int count)
		{
			uint[] result = new uint[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = unchecked((uint)random.Next());
			}
			return result;
		}

		[Test]
		public void VertexAssignmentSymmetry()
		{
			CompressedMesh_5_0_0_f4 compressedMesh = new();
			compressedMesh.SetVertices(vectors);
			Vector3[] unpackedValues = compressedMesh.GetVertices();
			AreAlmostEqual(vectors, unpackedValues, 0.000001f);
		}

		[Test]
		public void TriangleAssignmentSymmetry()
		{
			CompressedMesh_5_0_0_f4 compressedMesh = new();
			compressedMesh.SetTriangles(integers);
			uint[] unpackedValues = compressedMesh.GetTriangles();
			Assert.AreEqual(integers, unpackedValues);
		}

		private static void AreAlmostEqual(Vector3[] expected, Vector3[] actual, float maxDeviation)
		{
			if (expected.Length != actual.Length)
			{
				Assert.Fail($"Lengths were inequal.\nExpected: {expected.Length}\nBut was: {actual.Length}");
			}

			for (int i = 0; i < expected.Length; i++)
			{
				if (Vector3.Distance(expected[i], actual[i]) > maxDeviation)
				{
					Assert.Fail($"Values significantly differ at index {i}\nExpected: {expected[i]}\nBut was: {actual[i]}");
				}
			}
		}
	}
}
