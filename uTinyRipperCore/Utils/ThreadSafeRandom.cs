using System;

namespace uTinyRipper
{
	public class ThreadSafeRandom
	{
		public ThreadSafeRandom()
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
		}

		public int Next()
		{
			return s_local.Next();
		}

		public int Next(int maxValue)
		{
			return s_local.Next(maxValue);
		}

		public int Next(int minValue, int maxValue)
		{
			return s_local.Next(minValue, maxValue);
		}

		public void NextBytes(byte[] buffer)
		{
			s_local.NextBytes(buffer);
		}

		private static readonly Random s_global = new Random();
		[ThreadStatic]
		private static Random s_local;
	}
}
