using AssetRipper.Numerics;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class MeshHelper
	{
		public enum VertexChannelFormat
		{
			kChannelFormatFloat,
			kChannelFormatFloat16,
			kChannelFormatColor,
			kChannelFormatByte,
			kChannelFormatUInt32
		}

		public enum VertexFormat2017
		{
			kVertexFormatFloat,
			kVertexFormatFloat16,
			kVertexFormatColor,
			kVertexFormatUNorm8,
			kVertexFormatSNorm8,
			kVertexFormatUNorm16,
			kVertexFormatSNorm16,
			kVertexFormatUInt8,
			kVertexFormatSInt8,
			kVertexFormatUInt16,
			kVertexFormatSInt16,
			kVertexFormatUInt32,
			kVertexFormatSInt32
		}

		public enum VertexFormat
		{
			kVertexFormatFloat,
			kVertexFormatFloat16,
			kVertexFormatUNorm8,
			kVertexFormatSNorm8,
			kVertexFormatUNorm16,
			kVertexFormatSNorm16,
			kVertexFormatUInt8,
			kVertexFormatSInt8,
			kVertexFormatUInt16,
			kVertexFormatSInt16,
			kVertexFormatUInt32,
			kVertexFormatSInt32
		}

		public static VertexFormat ToVertexFormat(int format, UnityVersion version)
		{
			if (version.LessThan(2017))
			{
				return (VertexChannelFormat)format switch
				{
					VertexChannelFormat.kChannelFormatFloat => VertexFormat.kVertexFormatFloat,
					VertexChannelFormat.kChannelFormatFloat16 => VertexFormat.kVertexFormatFloat16,
					//in 4.x is size 4
					VertexChannelFormat.kChannelFormatColor => VertexFormat.kVertexFormatUNorm8,
					VertexChannelFormat.kChannelFormatByte => VertexFormat.kVertexFormatUInt8,
					//in 5.x
					VertexChannelFormat.kChannelFormatUInt32 => VertexFormat.kVertexFormatUInt32,
					_ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
				};
			}
			else if (version.LessThan(2019))
			{
				return (VertexFormat2017)format switch
				{
					VertexFormat2017.kVertexFormatFloat => VertexFormat.kVertexFormatFloat,
					VertexFormat2017.kVertexFormatFloat16 => VertexFormat.kVertexFormatFloat16,
					VertexFormat2017.kVertexFormatColor or VertexFormat2017.kVertexFormatUNorm8 => VertexFormat.kVertexFormatUNorm8,
					VertexFormat2017.kVertexFormatSNorm8 => VertexFormat.kVertexFormatSNorm8,
					VertexFormat2017.kVertexFormatUNorm16 => VertexFormat.kVertexFormatUNorm16,
					VertexFormat2017.kVertexFormatSNorm16 => VertexFormat.kVertexFormatSNorm16,
					VertexFormat2017.kVertexFormatUInt8 => VertexFormat.kVertexFormatUInt8,
					VertexFormat2017.kVertexFormatSInt8 => VertexFormat.kVertexFormatSInt8,
					VertexFormat2017.kVertexFormatUInt16 => VertexFormat.kVertexFormatUInt16,
					VertexFormat2017.kVertexFormatSInt16 => VertexFormat.kVertexFormatSInt16,
					VertexFormat2017.kVertexFormatUInt32 => VertexFormat.kVertexFormatUInt32,
					VertexFormat2017.kVertexFormatSInt32 => VertexFormat.kVertexFormatSInt32,
					_ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
				};
			}
			else
			{
				return (VertexFormat)format;
			}
		}


		public static int GetFormatSize(VertexFormat format)
		{
			switch (format)
			{
				case VertexFormat.kVertexFormatFloat:
				case VertexFormat.kVertexFormatUInt32:
				case VertexFormat.kVertexFormatSInt32:
					return 4;
				case VertexFormat.kVertexFormatFloat16:
				case VertexFormat.kVertexFormatUNorm16:
				case VertexFormat.kVertexFormatSNorm16:
				case VertexFormat.kVertexFormatUInt16:
				case VertexFormat.kVertexFormatSInt16:
					return 2;
				case VertexFormat.kVertexFormatUNorm8:
				case VertexFormat.kVertexFormatSNorm8:
				case VertexFormat.kVertexFormatUInt8:
				case VertexFormat.kVertexFormatSInt8:
					return 1;
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		public static bool IsIntFormat(VertexFormat format)
		{
			return format >= VertexFormat.kVertexFormatUInt8;
		}

		public static float[] BytesToFloatArray(ReadOnlySpan<byte> inputBytes, VertexFormat format)
		{
			int size = GetFormatSize(format);
			if (inputBytes.Length % size != 0)
			{
				throw new Exception($"Input array length {inputBytes.Length} is not divisible by size {size}");
			}

			int len = inputBytes.Length / size;
			float[] result = new float[len];
			for (int i = 0; i < len; i++)
			{
				switch (format)
				{
					case VertexFormat.kVertexFormatFloat:
						result[i] = BinaryPrimitives.ReadSingleLittleEndian(inputBytes[(i * sizeof(float))..]);
						break;
					case VertexFormat.kVertexFormatFloat16:
						result[i] = (float)BinaryPrimitives.ReadHalfLittleEndian(inputBytes[(i * Unsafe.SizeOf<Half>())..]);
						break;
					case VertexFormat.kVertexFormatUNorm8:
						result[i] = inputBytes[i] / 255f;
						break;
					case VertexFormat.kVertexFormatSNorm8:
						result[i] = Math.Max(unchecked((sbyte)inputBytes[i]) / 127f, -1f);
						break;
					case VertexFormat.kVertexFormatUNorm16:
						result[i] = BinaryPrimitives.ReadUInt16LittleEndian(inputBytes[(i * sizeof(ushort))..]) / 65535f;
						break;
					case VertexFormat.kVertexFormatSNorm16:
						result[i] = Math.Max(BinaryPrimitives.ReadInt16LittleEndian(inputBytes[(i * sizeof(short))..]) / 32767f, -1f);
						break;
				}
			}
			return result;
		}

		public static int[] BytesToIntArray(ReadOnlySpan<byte> inputBytes, VertexFormat format)
		{
			int size = GetFormatSize(format);
			int len = inputBytes.Length / size;
			int[] result = new int[len];
			for (int i = 0; i < len; i++)
			{
				switch (format)
				{
					case VertexFormat.kVertexFormatUInt8:
					case VertexFormat.kVertexFormatSInt8:
						result[i] = inputBytes[i];
						break;
					case VertexFormat.kVertexFormatUInt16:
					case VertexFormat.kVertexFormatSInt16:
						result[i] = BinaryPrimitives.ReadInt16LittleEndian(inputBytes[(i * sizeof(short))..]);
						break;
					case VertexFormat.kVertexFormatUInt32:
					case VertexFormat.kVertexFormatSInt32:
						result[i] = BinaryPrimitives.ReadInt32LittleEndian(inputBytes[(i * sizeof(int))..]);
						break;
				}
			}
			return result;
		}

		public static Vector2[] FloatArrayToVector2(float[] input) => FloatArrayToVector2(input, 2);
		public static Vector2[] FloatArrayToVector2(float[] input, int dimension)
		{
			ArgumentNullException.ThrowIfNull(input);

			ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);

			if (input.Length % dimension != 0)
			{
				throw new ArgumentException($"Input array length {input.Length} is not divisible by dimension {dimension}", nameof(input));
			}

			Vector2[] result = new Vector2[input.Length / dimension];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = dimension switch
				{
					1 => new Vector2(input[dimension * i], 0),
					_ => new Vector2(input[dimension * i], input[dimension * i + 1]),
				};
			}
			return result;
		}

		public static Vector3[] FloatArrayToVector3(float[] input) => FloatArrayToVector3(input, 3);
		public static Vector3[] FloatArrayToVector3(float[] input, int dimension)
		{
			ArgumentNullException.ThrowIfNull(input);

			ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);

			if (input.Length % dimension != 0)
			{
				throw new ArgumentException($"Input array length {input.Length} is not divisible by dimension {dimension}", nameof(input));
			}

			Vector3[] result = new Vector3[input.Length / dimension];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = dimension switch
				{
					1 => new Vector3(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]),
					2 => new Vector3(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]),
					_ => new Vector3(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]), //In the four dimensional case for Normals, the fourth dimension was always zero
																												//This is seemingly intended to maintain data alignment
				};
			}
			return result;
		}

		public static Vector4[] FloatArrayToVector4(float[] input) => FloatArrayToVector4(input, 4);
		public static Vector4[] FloatArrayToVector4(float[] input, int dimension)
		{
			ArgumentNullException.ThrowIfNull(input);

			ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);

			if (input.Length % dimension != 0)
			{
				throw new ArgumentException($"Input array length {input.Length} is not divisible by dimension {dimension}", nameof(input));
			}

			Vector4[] result = new Vector4[input.Length / dimension];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = dimension switch
				{
					1 => new Vector4(input[dimension * i], 0, 0, 0),
					2 => new Vector4(input[dimension * i], input[dimension * i + 1], 0, 0),
					3 => new Vector4(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2], 0),
					_ => new Vector4(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2], input[dimension * i + 3]),
				};
			}
			return result;
		}

		public static ColorFloat[] FloatArrayToColorFloat(float[] input)
		{
			ArgumentNullException.ThrowIfNull(input);

			if (input.Length % 4 != 0)
			{
				throw new ArgumentException($"Input array length {input.Length} is not divisible by four", nameof(input));
			}

			ColorFloat[] result = new ColorFloat[input.Length / 4];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = new ColorFloat(input[4 * i], input[4 * i + 1], input[4 * i + 2], input[4 * i + 3]);
			}
			return result;
		}
	}
}
