using System;

namespace uTinyRipper
{
	public static class RunetimeUtils
	{
		static RunetimeUtils()
		{
			IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
		}

		public static bool IsRunningOnMono { get; }
	}
}
