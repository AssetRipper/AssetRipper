using System;

namespace uTinyRipper
{
	public static class ArrayWriteAssetExtensions
	{
		public static void Write(this bool[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this char[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this byte[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this short[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this ushort[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this int[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this uint[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this long[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this ulong[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this float[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this double[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this string[] _this, AssetWriter writer)
		{
			ArrayWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write<T>(this Tuple<string, T>[] _this, AssetWriter writer)
			where T : IAssetWritable
		{
			writer.Write(_this.Length);
			for (int i = 0; i < _this.Length; i++)
			{
				writer.Write(_this[i]);
			}
		}

		public static void Write<T1, T2>(this Tuple<T1, T2>[] _this, AssetWriter writer, Func<T1, int> converter)
			where T2 : IAssetWritable
		{
			writer.Write(_this.Length);
			for (int i = 0; i < _this.Length; i++)
			{
				writer.Write(_this[i], converter);
			}
		}

		public static void Write<T>(this T[] _this, AssetWriter writer)
			   where T : IAssetWritable
		{
			writer.WriteAssetArray(_this);
		}

		public static void Write<T>(this T[][] _this, AssetWriter writer)
			   where T : IAssetWritable
		{
			writer.WriteAssetArray(_this);
		}
	}
}
