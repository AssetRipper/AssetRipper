using System;
using uTinyRipper;

namespace uTinyRipperConsole
{
	public class ConsoleLogger : ILogger
	{
		public ConsoleLogger()
		{
			if (IsConsoleConfigAvailable)
			{
				try
				{
					Console.WindowWidth = (int)(Console.LargestWindowWidth * 0.8f);
					Console.BufferHeight = 2000;
				}
				catch
				{
					// pull/563 : happens when running in any context where the console is not actually attached to a TTY
				}
			}
		}

		public void Log(LogType type, LogCategory category, string message)
		{
#if !DEBUG
			if(category == LogCategory.Debug)
			{
				return;
			}
#endif

			ConsoleColor backColor = Console.BackgroundColor;
			ConsoleColor foreColor = Console.ForegroundColor;

			switch (type)
			{
				case LogType.Info:
					Console.ForegroundColor = ConsoleColor.Gray;
					break;

				case LogType.Debug:
					Console.ForegroundColor = ConsoleColor.DarkGray;
					break;

				case LogType.Warning:
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					break;

				case LogType.Error:
					Console.ForegroundColor = ConsoleColor.DarkRed;
					break;
			}

			Console.WriteLine($"{category}: {message}");

			Console.BackgroundColor = backColor;
			Console.ForegroundColor = foreColor;
		}

		public static ConsoleLogger Instance { get; } = new ConsoleLogger();

		private static bool IsConsoleConfigAvailable
		{
			get
			{
#if NET_CORE
				return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
#else
				return !RunetimeUtils.IsRunningOnMono;
#endif
			}
		}
	}
}
