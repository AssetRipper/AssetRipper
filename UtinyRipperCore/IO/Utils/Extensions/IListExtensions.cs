using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public static class IListExtensions
	{
		public static void Read<T>(this IList<T> _this, AssetStream stream)
			where T : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T value = new T();
				value.Read(stream);
				_this.Add(value);
			}
		}

		public static void Read<T>(this IList<T> _this, AssetStream stream, Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T value = instantiator();
				value.Read(stream);
				_this.Add(value);
			}
		}
	}
}
