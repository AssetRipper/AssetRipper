using System;

namespace uTinyRipper
{
	public static class AssetWriterExtensions
	{
		public static void Write(this AssetWriter _this, Tuple<int, long> value)
		{
			EndianWriterExtensions.Write(_this, value);
		}

		public static void Write<T>(this AssetWriter _this, Tuple<T, long> value, Func<T, int> converter)
		{
			EndianWriterExtensions.Write(_this, value, converter);
		}

		public static void Write<T>(this AssetWriter _this, Tuple<string, T> value)
			where T : IAssetWritable
		{
			_this.Write(value.Item1);
			value.Item2.Write(_this);
		}

		public static void Write<T1, T2>(this AssetWriter _this, Tuple<T1, T2> value, Func<T1, int> converter)
			where T2 : IAssetWritable
		{
			_this.Write(converter(value.Item1));
			value.Item2.Write(_this);
		}
	}
}
