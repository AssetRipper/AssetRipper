using System.Collections.Generic;

namespace uTinyRipper
{
	public static class IDictionaryWriteEndianExtensions
	{
		public static void Write(this IReadOnlyDictionary<int, int> _this, EndianWriter writer)
		{
			writer.Write(_this.Count);
			foreach (KeyValuePair<int, int> kvp in _this)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value);
			}
		}
	}
}
