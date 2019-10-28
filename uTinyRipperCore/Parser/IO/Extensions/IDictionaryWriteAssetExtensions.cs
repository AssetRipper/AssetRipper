using System.Collections.Generic;

namespace uTinyRipper
{
	public static class IDictionaryWriteAssetExtensions
	{
		public static void Write(this IReadOnlyDictionary<int, int> _this, AssetWriter writer)
		{
			IDictionaryWriteEndianExtensions.Write(_this, writer);
		}

		public static void Write<T1, T2>(this IReadOnlyDictionary<T1, T2> _this, AssetWriter writer)
			where T1 : IAssetWritable, new()
			where T2 : IAssetWritable, new()
		{
			writer.Write(_this.Count);
			foreach (KeyValuePair<T1, T2> kvp in _this)
			{
				kvp.Key.Write(writer);
				kvp.Value.Write(writer);
			}
		}
	}
}
