using AssetRipper.IO.Endian;

namespace AssetRipper.Core.IO.Extensions
{
	public static class ArrayWriteEndianExtensions
	{
		public static void Write(this bool[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this char[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this byte[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this short[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this ushort[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this int[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this uint[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this long[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this ulong[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this float[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this double[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}

		public static void Write(this string[] _this, EndianWriter writer)
		{
			writer.WriteArray(_this);
		}
	}
}
