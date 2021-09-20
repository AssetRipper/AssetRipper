using AssetRipper.Core.Math;
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
				switch ((VertexChannelFormat)format)
				{
					case VertexChannelFormat.kChannelFormatFloat:
						return VertexFormat.kVertexFormatFloat;
					case VertexChannelFormat.kChannelFormatFloat16:
						return VertexFormat.kVertexFormatFloat16;
					case VertexChannelFormat.kChannelFormatColor: //in 4.x is size 4
						return VertexFormat.kVertexFormatUNorm8;
					case VertexChannelFormat.kChannelFormatByte:
						return VertexFormat.kVertexFormatUInt8;
					case VertexChannelFormat.kChannelFormatUInt32: //in 5.x
						return VertexFormat.kVertexFormatUInt32;
					default:
						throw new ArgumentOutOfRangeException(nameof(format), format, null);
				}
			}
			else if (version.IsLess(2019))
			{
				switch ((VertexFormat2017)format)
				{
					case VertexFormat2017.kVertexFormatFloat:
						return VertexFormat.kVertexFormatFloat;
					case VertexFormat2017.kVertexFormatFloat16:
						return VertexFormat.kVertexFormatFloat16;
					case VertexFormat2017.kVertexFormatColor:
					case VertexFormat2017.kVertexFormatUNorm8:
						return VertexFormat.kVertexFormatUNorm8;
					case VertexFormat2017.kVertexFormatSNorm8:
						return VertexFormat.kVertexFormatSNorm8;
					case VertexFormat2017.kVertexFormatUNorm16:
						return VertexFormat.kVertexFormatUNorm16;
					case VertexFormat2017.kVertexFormatSNorm16:
						return VertexFormat.kVertexFormatSNorm16;
					case VertexFormat2017.kVertexFormatUInt8:
						return VertexFormat.kVertexFormatUInt8;
					case VertexFormat2017.kVertexFormatSInt8:
						return VertexFormat.kVertexFormatSInt8;
					case VertexFormat2017.kVertexFormatUInt16:
						return VertexFormat.kVertexFormatUInt16;
					case VertexFormat2017.kVertexFormatSInt16:
						return VertexFormat.kVertexFormatSInt16;
					case VertexFormat2017.kVertexFormatUInt32:
						return VertexFormat.kVertexFormatUInt32;
					case VertexFormat2017.kVertexFormatSInt32:
						return VertexFormat.kVertexFormatSInt32;
					default:
						throw new ArgumentOutOfRangeException(nameof(format), format, null);
				}
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
						result[i] = (float) BitConverter.ToHalf(inputBytes, i * 2);
						break;
					case VertexFormat.kVertexFormatUNorm8:
						result[i] = inputBytes[i] / 255f;
						break;
					case VertexFormat.kVertexFormatSNorm8:
						result[i] = System.Math.Max((sbyte)inputBytes[i] / 127f, -1f);
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
			for(int i = 0; i < result.Length; i++)
			{
				switch (dimension)
				{
					case 1:
						result[i] = new Vector2f(input[dimension * i], 0);
						break;
					default:
						result[i] = new Vector2f(input[dimension * i], input[dimension * i + 1]);
						break;
				}
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
				switch (dimension)
				{
					case 1:
						result[i] = new Vector3f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]);
						break;
					case 2:
						result[i] = new Vector3f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]);
						break;
					default:
						//In the four dimensional case for Normals, the fourth dimension was always zero
						//This is seemingly intended to maintain data alignment
						result[i] = new Vector3f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2]);
						break;
				}
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
				switch (dimension)
				{
					case 1:
						result[i] = new Vector4f(input[dimension * i], 0, 0, 0);
						break;
					case 2:
						result[i] = new Vector4f(input[dimension * i], input[dimension * i + 1], 0, 0);
						break;
					case 3:
						result[i] = new Vector4f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2], 0);
						break;
					default:
						result[i] = new Vector4f(input[dimension * i], input[dimension * i + 1], input[dimension * i + 2], input[dimension * i + 3]);
						break;
				}
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
