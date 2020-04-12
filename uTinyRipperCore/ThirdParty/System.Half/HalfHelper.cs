using System.Runtime.InteropServices;

namespace System
{
	/// <summary>
	/// Helper class for Half conversions and some low level operations.
	/// This class is internally used in the Half class.
	/// </summary>
	/// <remarks>
	/// References:
	///     - Fast Half Float Conversions, Jeroen van der Zijp, link: http://www.fox-toolkit.org/ftp/fasthalffloatconversion.pdf
	/// </remarks>
	[ComVisible(false)]
	internal static class HalfHelper
	{
		private static readonly uint[] mantissaTable = GenerateMantissaTable();
		private static readonly uint[] exponentTable = GenerateExponentTable();
		private static readonly ushort[] offsetTable = GenerateOffsetTable();
		private static readonly ushort[] baseTable = GenerateBaseTable();
		private static readonly sbyte[] shiftTable = GenerateShiftTable();

		// Transforms the subnormal representation to a normalized one. 
		private static uint ConvertMantissa(uint i)
		{
			uint m = i << 13; // Zero pad mantissa bits
			uint e = 0; // Zero exponent

			// While not normalized
			while ((m & 0x00800000U) == 0)
			{
				e = unchecked(e - 0x00800000U); // Decrement exponent (1<<23)
				m <<= 1; // Shift mantissa                
			}
			m &= ~0x00800000U; // Clear leading 1 bit
			e = unchecked(e + 0x38800000U); // Adjust bias ((127-14)<<23)
			return m | e; // Return combined number
		}

		private static uint[] GenerateMantissaTable()
		{
			uint[] mantissaTable = new uint[2048];
			mantissaTable[0] = 0;
			for (uint i = 1; i < 1024; i++)
			{
				mantissaTable[i] = ConvertMantissa(i);
			}
			for (uint i = 1024; i < 2048; i++)
			{
				mantissaTable[i] = 0x38000000U + ((i - 1024) << 13);
			}

			return mantissaTable;
		}
		private static uint[] GenerateExponentTable()
		{
			uint[] exponentTable = new uint[64];
			exponentTable[0] = 0;
			for (uint i = 1; i < 31; i++)
			{
				exponentTable[i] = i << 23;
			}
			exponentTable[31] = 0x47800000U;
			exponentTable[32] = 0x80000000U;
			for (uint i = 33; i < 63; i++)
			{
				exponentTable[i] = 0x80000000U + ((i - 32) << 23);
			}
			exponentTable[63] = 0xc7800000U;

			return exponentTable;
		}
		private static ushort[] GenerateOffsetTable()
		{
			ushort[] offsetTable = new ushort[64];
			offsetTable[0] = 0;
			for (int i = 1; i < 32; i++)
			{
				offsetTable[i] = 1024;
			}
			offsetTable[32] = 0;
			for (int i = 33; i < 64; i++)
			{
				offsetTable[i] = 1024;
			}

			return offsetTable;
		}
		private static ushort[] GenerateBaseTable()
		{
			ushort[] baseTable = new ushort[512];
			for (int i = 0; i < 256; ++i)
			{
				sbyte e = (sbyte)(127 - i);
				if (e > 24)
				{ // Very small numbers map to zero
					baseTable[i | 0x000] = 0x0000;
					baseTable[i | 0x100] = 0x8000;
				}
				else if (e > 14)
				{ // Small numbers map to denorms
					baseTable[i | 0x000] = (ushort)(0x0400 >> (18 + e));
					baseTable[i | 0x100] = (ushort)((0x0400 >> (18 + e)) | 0x8000);
				}
				else if (e >= -15)
				{ // Normal numbers just lose precision
					baseTable[i | 0x000] = (ushort)((15 - e) << 10);
					baseTable[i | 0x100] = (ushort)(((15 - e) << 10) | 0x8000);
				}
				else if (e > -128)
				{ // Large numbers map to Infinity
					baseTable[i | 0x000] = 0x7c00;
					baseTable[i | 0x100] = 0xfc00;
				}
				else
				{ // Infinity and NaN's stay Infinity and NaN's
					baseTable[i | 0x000] = 0x7c00;
					baseTable[i | 0x100] = 0xfc00;
				}
			}

			return baseTable;
		}
		private static sbyte[] GenerateShiftTable()
		{
			sbyte[] shiftTable = new sbyte[512];
			for (int i = 0; i < 256; ++i)
			{
				sbyte e = (sbyte)(127 - i);
				if (e > 24)
				{ // Very small numbers map to zero
					shiftTable[i | 0x000] = 24;
					shiftTable[i | 0x100] = 24;
				}
				else if (e > 14)
				{ // Small numbers map to denorms
					shiftTable[i | 0x000] = (sbyte)(e - 1);
					shiftTable[i | 0x100] = (sbyte)(e - 1);
				}
				else if (e >= -15)
				{ // Normal numbers just lose precision
					shiftTable[i | 0x000] = 13;
					shiftTable[i | 0x100] = 13;
				}
				else if (e > -128)
				{ // Large numbers map to Infinity
					shiftTable[i | 0x000] = 24;
					shiftTable[i | 0x100] = 24;
				}
				else
				{ // Infinity and NaN's stay Infinity and NaN's
					shiftTable[i | 0x000] = 13;
					shiftTable[i | 0x100] = 13;
				}
			}

			return shiftTable;
		}

		public static float HalfToSingle(Half half)
		{
			uint result = mantissaTable[offsetTable[half.value >> 10] + (half.value & 0x3ff)] + exponentTable[half.value >> 10];
			byte[] uintBytes = BitConverter.GetBytes(result);
			return BitConverter.ToSingle(uintBytes, 0);
		}
		public static Half SingleToHalf(float single)
		{
			byte[] singleBytes = BitConverter.GetBytes(single);
			uint value = BitConverter.ToUInt32(singleBytes, 0);
			ushort result = (ushort)(baseTable[(value >> 23) & 0x1ff] + ((value & 0x007fffff) >> shiftTable[value >> 23]));
			return Half.ToHalf(result);
		}

		public static Half Negate(Half half)
		{
			return Half.ToHalf((ushort)(half.value ^ 0x8000));
		}
		public static Half Abs(Half half)
		{
			return Half.ToHalf((ushort)(half.value & 0x7fff));
		}

		public static bool IsNaN(Half half)
		{
			return ((half.value & 0x7fff) > 0x7c00);
		}
		public static bool IsInfinity(Half half)
		{
			return ((half.value & 0x7fff) == 0x7c00);
		}
		public static bool IsPositiveInfinity(Half half)
		{
			return (half.value == 0x7c00);
		}
		public static bool IsNegativeInfinity(Half half)
		{
			return (half.value == 0xfc00);
		}
	}
}