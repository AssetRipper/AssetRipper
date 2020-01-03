using System;
using System.Collections.Generic;

namespace uTinyRipper
{
	public static class IDictionaryWriteEndianExtensions
	{
		public static void Write(this IReadOnlyDictionary<int, int> _this, EndianWriter writer)
		{
			writer.Write(_this.Count);
			foreach (var kvp in _this)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value);
			}
		}

		public static void Write(this IReadOnlyDictionary<int, string> _this, EndianWriter writer)
		{
			writer.Write(_this.Count);
			foreach (var kvp in _this)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value);
			}
		}

		public static void Write(this IReadOnlyDictionary<long, string> _this, EndianWriter writer)
		{
			writer.Write(_this.Count);
			foreach (var kvp in _this)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value);
			}
		}

		public static void Write(this IReadOnlyDictionary<Tuple<int, long>, string> _this, EndianWriter writer)
		{
			writer.Write(_this.Count);
			foreach (var kvp in _this)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value);
			}
		}

		public static void Write<T>(this IReadOnlyDictionary<Tuple<T, long>, string> _this, EndianWriter writer, Func<T, int> converter)
		{
			writer.Write(_this.Count);
			foreach (var kvp in _this)
			{
				writer.Write(kvp.Key, converter);
				writer.Write(kvp.Value);
			}
		}
	}
}
