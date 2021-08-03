using System;
using System.Runtime.InteropServices;

namespace AssetRipper.Core.Utils
{
	public static class RunetimeUtils
	{
		static RunetimeUtils()
		{
			IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
			IsRunningOnNetCore = RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.Ordinal);

			if (OperatingSystem.IsWindows()) RuntimeOS = RuntimeOperatingSystem.Windows;
			else if (OperatingSystem.IsLinux()) RuntimeOS = RuntimeOperatingSystem.Linux;
			else if (OperatingSystem.IsMacOS()) RuntimeOS = RuntimeOperatingSystem.MacOS;
			else RuntimeOS = RuntimeOperatingSystem.Other;
		}

		public static bool IsRunningOnMono { get; }
		public static bool IsRunningOnNetCore { get; }
		public static RuntimeOperatingSystem RuntimeOS { get; }
	}
}
