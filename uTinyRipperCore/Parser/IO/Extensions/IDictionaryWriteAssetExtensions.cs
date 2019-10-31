using System;
using System.Collections.Generic;

namespace uTinyRipper
{
	public static class IDictionaryWriteAssetExtensions
	{
		public static void Write(this IReadOnlyDictionary<int, int> _this, AssetWriter writer)
		{
			IDictionaryWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this IReadOnlyDictionary<int, string> _this, AssetWriter writer)
		{
			IDictionaryWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this IReadOnlyDictionary<long, string> _this, AssetWriter writer)
		{
			IDictionaryWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write(this IReadOnlyDictionary<Tuple<int, long>, string> _this, AssetWriter writer)
		{
			IDictionaryWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write<T>(this IReadOnlyDictionary<Tuple<T, long>, string> _this, AssetWriter writer, Func<T, int> converter)
		{
			IDictionaryWriteEndianExtensions.Write(_this, writer, converter);
		}

		public static void Write<T>(this IReadOnlyDictionary<string, T> _this, AssetWriter writer)
			where T : IAssetWritable
		{
			writer.Write(_this.Count);
			foreach (var kvp in _this)
			{
				writer.Write(kvp.Key);
				kvp.Value.Write(writer);
			}
		}

		public static void Write<T1, T2>(this IReadOnlyDictionary<T1, T2> _this, AssetWriter writer)
			where T1 : IAssetWritable
			where T2 : IAssetWritable
		{
			writer.Write(_this.Count);
			foreach (var kvp in _this)
			{
				kvp.Key.Write(writer);
				kvp.Value.Write(writer);
			}
		}
	}
}
