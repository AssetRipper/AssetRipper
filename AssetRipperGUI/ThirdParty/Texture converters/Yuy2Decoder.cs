using System.Runtime.CompilerServices;

namespace Yuy2
{
	public static class Yuy2Decoder
	{
		public static byte[] DecompressYUY2(byte[] input, int width, int height)
		{
			byte[] output = new byte[width * height * sizeof(uint)];
			DecompressYUY2(input, width, height, output);
			return output;
		}

		public unsafe static void DecompressYUY2(byte[] input, int width, int height, byte[] output)
		{
			fixed (byte* pInput = input)
			{
				fixed (byte* pOutput = output)
				{
					DecompressYUY2(pInput, width, height, pOutput);
				}
			}
		}

		public unsafe static void DecompressYUY2(byte* input, int width, int height, byte* output)
		{
			int halfWidth = width / 2;
			for (int j = 0; j < height; j++)
			{
				for (int i = 0; i < halfWidth; ++i)
				{
					int y0 = input[0];
					int u0 = input[1];
					int y1 = input[2];
					int v0 = input[3];
					input += 4;
					int c = y0 - 16;
					int d = u0 - 128;
					int e = v0 - 128;
					output[0] = ClampByte((298 * c + 516 * d + 128) >> 8);            // blue
					output[1] = ClampByte((298 * c - 100 * d - 208 * e + 128) >> 8);  // green
					output[2] = ClampByte((298 * c + 409 * e + 128) >> 8);            // red
					output[3] = 255;
					output += 4;
					c = y1 - 16;
					output[0] = ClampByte((298 * c + 516 * d + 128) >> 8);            // blue
					output[1] = ClampByte((298 * c - 100 * d - 208 * e + 128) >> 8);  // green
					output[2] = ClampByte((298 * c + 409 * e + 128) >> 8);            // red
					output[3] = 255;
					output += 4;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte ClampByte(int x)
		{
			return (byte)(byte.MaxValue < x ? byte.MaxValue : (x > byte.MinValue ? x : byte.MinValue));
		}
	}
}
