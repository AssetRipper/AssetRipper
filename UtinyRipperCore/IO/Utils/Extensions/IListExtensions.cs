using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public static class IListExtensions
	{
		public static void Read<T>(this IList<T> _this, AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T value = new T();
				value.Read(reader);
				_this.Add(value);
			}
		}

		public static void Read<T>(this IList<T> _this, AssetReader reader, Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T value = instantiator();
				value.Read(reader);
				_this.Add(value);
			}
		}
	}
}
