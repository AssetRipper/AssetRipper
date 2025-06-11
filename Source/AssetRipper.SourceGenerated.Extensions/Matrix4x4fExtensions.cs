using AssetRipper.SourceGenerated.Subclasses.Matrix4x4f;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public static class Matrix4x4fExtensions
{
	extension(Matrix4x4f matrix)
	{
		// Note: Matrix4x4f and Matrix4x4 are not the same! Their memory layouts are different.
		// They need to be transposed because M14 maps to m30 in the field layouts. The necessity of transposition can be verified with vector math.

		public void CopyValues(Matrix4x4 source)
		{
			matrix.SetValues(
				source.M11,
				source.M21,
				source.M31,
				source.M41,
				source.M12,
				source.M22,
				source.M32,
				source.M42,
				source.M13,
				source.M23,
				source.M33,
				source.M43,
				source.M14,
				source.M24,
				source.M34,
				source.M44);
		}

		public Matrix4x4 CastToStruct()
		{
			return new Matrix4x4(
				matrix.E00,
				matrix.E10,
				matrix.E20,
				matrix.E30,
				matrix.E01,
				matrix.E11,
				matrix.E21,
				matrix.E31,
				matrix.E02,
				matrix.E12,
				matrix.E22,
				matrix.E32,
				matrix.E03,
				matrix.E13,
				matrix.E23,
				matrix.E33);
		}
	}
}
