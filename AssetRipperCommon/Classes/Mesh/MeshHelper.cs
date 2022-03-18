using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using System;

namespace AssetRipper.Core.Classes.Mesh
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
			if (version.IsLess(2017))
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
			else if (version.IsLess(2019))
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


		public static uint GetFormatSize(VertexFormat format)
		{
			switch (format)
			{
				case VertexFormat.kVertexFormatFloat:
				case VertexFormat.kVertexFormatUInt32:
				case VertexFormat.kVertexFormatSInt32:
					return 4u;
				case VertexFormat.kVertexFormatFloat16:
				case VertexFormat.kVertexFormatUNorm16:
				case VertexFormat.kVertexFormatSNorm16:
				case VertexFormat.kVertexFormatUInt16:
				case VertexFormat.kVertexFormatSInt16:
					return 2u;
				case VertexFormat.kVertexFormatUNorm8:
				case VertexFormat.kVertexFormatSNorm8:
				case VertexFormat.kVertexFormatUInt8:
				case VertexFormat.kVertexFormatSInt8:
					return 1u;
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		public static bool IsIntFormat(VertexFormat format)
		{
			return format >= VertexFormat.kVertexFormatUInt8;
		}

		public static float[] BytesToFloatArray(byte[] inputBytes, VertexFormat format)
		{
			if (inputBytes == null)
				throw new ArgumentNullException(nameof(inputBytes));
			var size = GetFormatSize(format);
			if (inputBytes.Length % size != 0)
				throw new Exception($"Input array length {inputBytes.Length} is not divisible by size {size}");
			var len = inputBytes.Length / size;
			var result = new float[len];
			for (int i = 0; i < len; i++)
			{
				switch (format)
				{
					case VertexFormat.kVertexFormatFloat:
						result[i] = BitConverter.ToSingle(inputBytes, i * 4);
						break;
					case VertexFormat.kVertexFormatFloat16:
						result[i] = (float)BitConverter.ToHalf(inputBytes, i * 2);
						break;
					case VertexFormat.kVertexFormatUNorm8:
						result[i] = inputBytes[i] / 255f;
						break;
					case VertexFormat.kVertexFormatSNorm8:
						result[i] = System.Math.Max(unchecked((sbyte)inputBytes[i]) / 127f, -1f);
						break;
					case VertexFormat.kVertexFormatUNorm16:
						result[i] = BitConverter.ToUInt16(inputBytes, i * 2) / 65535f;
						break;
					case VertexFormat.kVertexFormatSNorm16:
						result[i] = System.Math.Max(BitConverter.ToInt16(inputBytes, i * 2) / 32767f, -1f);
						break;
				}
			}
			return result;
		}

		public static int[] BytesToIntArray(byte[] inputBytes, VertexFormat format)
		{
			var size = GetFormatSize(format);
			var len = inputBytes.Length / size;
			var result = new int[len];
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
						result[i] = BitConverter.ToInt16(inputBytes, i * 2);
						break;
					case VertexFormat.kVertexFormatUInt32:
					case VertexFormat.kVertexFormatSInt32:
						result[i] = BitConverter.ToInt32(inputBytes, i * 4);
						break;
				}
			}
			return result;
		}

		public static Vector2f[] FloatArrayToVector2(float[] input) => FloatArrayToVector2(input, 2);
		public static Vector2f[] FloatArrayToVector2(float[] input, int dimension)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (dimension < 1)
				throw new ArgumentOutOfRangeException(nameof(dimension));
			if (input.Length % dimension != 0)
				throw new ArgumentException($"Input array length {input.Length} is not divisible by dimension {dimension}", nameof(input));

			Vector2f[] result = new Vector2f[input.Length / dimension];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = dimension switch
				{
					1 => new Vector2f(input[dimension * i], 0),
					_ => new Vector2f(input[dimension * i], input[dimension * i + 1]),
				};
			}
			return result;
		}

		public static Vector3f[] FloatArrayToVector3(float[] input) => FloatArrayToVector3(input, 3);
		public static Vector3f[] FloatArrayToVector3(float[] input, int dimension)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (dimension < 1)
				throw new ArgumentOutOfRangeException(nameof(dimension));
			if (input.Length % dimension != 0)
				throw new ArgumentException($"Input array length {input.Length} is not divisible by dimension {dimension}", nameof(input));

			Vector3f[] result = new Vector3f[input.Length / dimension];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = dimension switch
				{
					1 => new Vector3f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]),
					2 => new Vector3f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]),
					_ => new Vector3f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]),//In the four dimensional case for Normals, the fourth dimension was always zero
																												//This is seemingly intended to maintain data alignment
				};
			}
			return result;
		}

		public static Vector4f[] FloatArrayToVector4(float[] input) => FloatArrayToVector4(input, 4);
		public static Vector4f[] FloatArrayToVector4(float[] input, int dimension)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (dimension < 1)
				throw new ArgumentOutOfRangeException(nameof(dimension));
			if (input.Length % dimension != 0)
				throw new ArgumentException($"Input array length {input.Length} is not divisible by dimension {dimension}", nameof(input));

			Vector4f[] result = new Vector4f[input.Length / dimension];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = dimension switch
				{
					1 => new Vector4f(input[dimension * i], 0, 0, 0),
					2 => new Vector4f(input[dimension * i], input[dimension * i + 1], 0, 0),
					3 => new Vector4f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2], 0),
					_ => new Vector4f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2], input[dimension * i + 3]),
				};
			}
			return result;
		}

		public static ColorRGBAf[] FloatArrayToColorRGBAf(float[] input)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (input.Length % 4 != 0)
				throw new ArgumentException($"Input array length {input.Length} is not divisible by four", nameof(input));

			ColorRGBAf[] result = new ColorRGBAf[input.Length / 4];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = new ColorRGBAf(input[4 * i], input[4 * i + 1], input[4 * i + 2], input[4 * i + 3]);
			}
			return result;
		}

		public static ColorRGBA32[] FloatArrayToColorRGBA32(float[] input)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (input.Length % 4 != 0)
				throw new ArgumentException($"Input array length {input.Length} is not divisible by four", nameof(input));

			ColorRGBA32[] result = new ColorRGBA32[input.Length / 4];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = (ColorRGBA32)new ColorRGBAf(input[4 * i], input[4 * i + 1], input[4 * i + 2], input[4 * i + 3]);
			}
			return result;
		}
	}
}
