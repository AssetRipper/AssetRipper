using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Extensions
{
	public static class RunetimeUtils
	{
		static RunetimeUtils()
		{
			IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
			IsRunningOnNetCore = RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.Ordinal);
		}

		public static bool IsRunningOnMono { get; }
		public static bool IsRunningOnNetCore { get; }
	}
}
