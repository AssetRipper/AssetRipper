using AssetRipper.Assets.IO.Writing;

namespace AssetRipper.Core.IO.Extensions
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

		public static void Write<T>(this T[] _this, AssetWriter writer)
			   where T : IAssetWritable
		{
			writer.WriteAssetArray(_this);
		}

		public static void WriteAssetArray<T>(this AssetWriter writer, T[] buffer) where T : IAssetWritable
		{
			writer.Write(buffer.Length);

			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i].Write(writer);
			}

			if (writer.IsAlignArray)
			{
				writer.AlignStream();
			}
		}

		public static void WriteAssetArray<T>(this AssetWriter writer, T[][] buffer) where T : IAssetWritable
		{
			writer.Write(buffer.Length);

			for (int i = 0; i < buffer.GetLength(0); i++)
			{
				writer.WriteAssetArray(buffer[i]);
			}

			if (writer.IsAlignArray)
			{
				writer.AlignStream();
			}
		}
	}
}
