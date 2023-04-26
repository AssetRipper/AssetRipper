using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetRipper.Tests
{
	internal sealed class RandomStructEnumerable<T> : IEnumerable<T> where T : unmanaged
	{
		private byte[] Data { get; }
		public int Count { get; }

		public RandomStructEnumerable(int count)
		{
			Count = count;
			Data = new byte[Unsafe.SizeOf<T>()];
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = Count; i > 0; i--)
			{
				TestContext.CurrentContext.Random.NextBytes(Data);
				yield return MemoryMarshal.Read<T>(Data);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
