using System;

namespace uTinyRipper
{
	public class ThreadSafeRandom
	{
		public int Next()
		{
			return GetLocal().Next();
		}

		public int Next(int maxValue)
		{
			return GetLocal().Next(maxValue);
		}

		public int Next(int minValue, int maxValue)
		{
			return GetLocal().Next(minValue, maxValue);
		}

		public void NextBytes(byte[] buffer)
		{
			GetLocal().NextBytes(buffer);
		}

		private static Random GetLocal()
		{
			if (s_local == null)
			{
				int seed;
				lock (s_global)
				{
					seed = s_global.Next();
				}
				s_local = new Random(seed);
			}
			return s_local;
		}

		private static readonly Random s_global = new Random();
		[ThreadStatic]
		private static Random s_local;
	}
}
