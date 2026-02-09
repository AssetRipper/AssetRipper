using AssetRipper.Numerics;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetRipper.SourceGenerated.Extensions;

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

	public static int ToChannelFormat(VertexFormat format, UnityVersion version)
	{
		if (version.LessThan(2017))
		{
			return format switch
			{
				VertexFormat.kVertexFormatFloat => (int)VertexChannelFormat.kChannelFormatFloat,
				VertexFormat.kVertexFormatFloat16 => (int)VertexChannelFormat.kChannelFormatFloat16,
				VertexFormat.kVertexFormatUNorm8 => (int)VertexChannelFormat.kChannelFormatColor,
				VertexFormat.kVertexFormatUInt8 => (int)VertexChannelFormat.kChannelFormatByte,
				VertexFormat.kVertexFormatUInt32 => (int)VertexChannelFormat.kChannelFormatUInt32,
				_ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
			};
		}
		else if (version.LessThan(2019))
		{
			return format switch
			{
				VertexFormat.kVertexFormatFloat => (int)VertexFormat2017.kVertexFormatFloat,
				VertexFormat.kVertexFormatFloat16 => (int)VertexFormat2017.kVertexFormatFloat16,
				VertexFormat.kVertexFormatUNorm8 => (int)VertexFormat2017.kVertexFormatUNorm8,
				VertexFormat.kVertexFormatSNorm8 => (int)VertexFormat2017.kVertexFormatSNorm8,
				VertexFormat.kVertexFormatUNorm16 => (int)VertexFormat2017.kVertexFormatUNorm16,
				VertexFormat.kVertexFormatSNorm16 => (int)VertexFormat2017.kVertexFormatSNorm16,
				VertexFormat.kVertexFormatUInt8 => (int)VertexFormat2017.kVertexFormatUInt8,
				VertexFormat.kVertexFormatSInt8 => (int)VertexFormat2017.kVertexFormatSInt8,
				VertexFormat.kVertexFormatUInt16 => (int)VertexFormat2017.kVertexFormatUInt16,
				VertexFormat.kVertexFormatSInt16 => (int)VertexFormat2017.kVertexFormatSInt16,
				VertexFormat.kVertexFormatUInt32 => (int)VertexFormat2017.kVertexFormatUInt32,
				VertexFormat.kVertexFormatSInt32 => (int)VertexFormat2017.kVertexFormatSInt32,
				_ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
			};
		}
		else
		{
			return (int)format;
		}
	}

	public static int GetChannelFormat_Color(UnityVersion version)
	{
		if (version.LessThan(2017))
		{
			return (int)VertexChannelFormat.kChannelFormatColor;
		}
		else if (version.LessThan(2019))
		{
			return (int)VertexFormat2017.kVertexFormatColor;
		}
		else
		{
			return (int)VertexFormat.kVertexFormatUNorm8;
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
		ArgumentOutOfRangeException.ThrowIfGreaterThan(dimension, 4);
		ValidateLength(input, dimension);

		Vector2[] result = GC.AllocateUninitializedArray<Vector2>(input.Length / dimension);
		switch (dimension)
		{
			case 1:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector2(input[i], 0);
				}
				break;
			case 2:
				MemoryMarshal.Cast<float, Vector2>(input).CopyTo(result);
				break;
			case 3:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector2(input[3 * i], input[3 * i + 1]);
				}
				break;
			case 4:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector2(input[4 * i], input[4 * i + 1]);
				}
				break;
		}
		return result;
	}

	public static Vector3[] FloatArrayToVector3(float[] input) => FloatArrayToVector3(input, 3);
	public static Vector3[] FloatArrayToVector3(float[] input, int dimension)
	{
		ArgumentNullException.ThrowIfNull(input);

		ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(dimension, 4);
		ValidateLength(input, dimension);

		//In the four dimensional case for Normals, the fourth dimension was always zero
		//This is seemingly intended to maintain data alignment

		Vector3[] result = GC.AllocateUninitializedArray<Vector3>(input.Length / dimension);
		switch (dimension)
		{
			case 1:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector3(input[i], 0, 0);
				}
				break;
			case 2:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector3(input[2 * i], input[2 * i + 1], 0);
				}
				break;
			case 3:
				MemoryMarshal.Cast<float, Vector3>(input).CopyTo(result);
				break;
			case 4:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector3(input[4 * i], input[4 * i + 1], input[4 * i + 2]);
				}
				break;
		}
		return result;
	}

	public static Vector4[] FloatArrayToVector4(float[] input) => FloatArrayToVector4(input, 4);
	public static Vector4[] FloatArrayToVector4(float[] input, int dimension)
	{
		ArgumentNullException.ThrowIfNull(input);

		ArgumentOutOfRangeException.ThrowIfLessThan(dimension, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(dimension, 4);
		ValidateLength(input, dimension);

		Vector4[] result = GC.AllocateUninitializedArray<Vector4>(input.Length / dimension);
		switch (dimension)
		{
			case 1:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector4(input[i], 0, 0, 0);
				}
				break;
			case 2:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector4(input[2 * i], input[2 * i + 1], 0, 0);
				}
				break;
			case 3:
				for (int i = result.Length - 1; i >= 0; i--)
				{
					result[i] = new Vector4(input[3 * i], input[3 * i + 1], input[3 * i + 2], 0);
				}
				break;
			case 4:
				MemoryMarshal.Cast<float, Vector4>(input).CopyTo(result);
				break;
		}
		return result;
	}

	private static void ValidateLength(float[] input, int dimension)
	{
		if (input.Length % dimension != 0)
		{
			throw new ArgumentException($"Input array length {input.Length} is not divisible by dimension {dimension}", nameof(input));
		}
	}

	public static ColorFloat[] FloatArrayToColorFloat(float[] input)
	{
		ArgumentNullException.ThrowIfNull(input);

		if (input.Length % 4 != 0)
		{
			throw new ArgumentException($"Input array length {input.Length} is not divisible by four", nameof(input));
		}

		return MemoryMarshal.Cast<float, ColorFloat>(input).ToArray();
	}
}
