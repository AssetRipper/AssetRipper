using AssetRipper.Core.Math;
using System;
using System.IO;

namespace AssetRipper.Core.IO.Extensions
{
	public static class BinaryMathExtensions
	{
		public static Quaternionf ReadQuaternionf(this BinaryReader reader)
		{
			return new Quaternionf(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Vector2f ReadVector2f(this BinaryReader reader)
		{
			return new Vector2f(reader.ReadSingle(), reader.ReadSingle());
		}

		public static Vector3f ReadVector3f(this BinaryReader reader)
		{
			return new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Vector4f ReadVector4f(this BinaryReader reader)
		{
			return new Vector4f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static ColorRGBAf ReadColor4(this BinaryReader reader)
		{
			return new ColorRGBAf(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Matrix4x4f ReadMatrix(this BinaryReader reader)
		{
			return new Matrix4x4f(reader.ReadSingleArray(16));
		}

		public static Vector2f[] ReadVector2Array(this BinaryReader reader)
		{
			return ReadArray(reader.ReadVector2f, reader.ReadInt32());
		}

		public static Vector4f[] ReadVector4Array(this BinaryReader reader)
		{
			return ReadArray(reader.ReadVector4f, reader.ReadInt32());
		}

		public static Matrix4x4f[] ReadMatrixArray(this BinaryReader reader)
		{
			return ReadArray(reader.ReadMatrix, reader.ReadInt32());
		}

		private static T[] ReadArray<T>(Func<T> del, int length)
		{
			var array = new T[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = del();
			}
			return array;
		}
	}
}
