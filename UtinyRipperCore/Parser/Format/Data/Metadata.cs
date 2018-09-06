using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public abstract class Metadata<T> : IDisposable
		where T: FileEntry
	{
		~Metadata()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			foreach (T entry in Entries)
			{
				entry.Dispose();
			}
		}

		public abstract IReadOnlyList<T> Entries { get; }
	}
}
