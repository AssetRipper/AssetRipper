using System;

namespace AssetRipper.Logging
{
	public class ConsoleLogger : ILogger
	{
		public ConsoleLogger()
		{
			if (OperatingSystem.IsWindows())
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

		public void BlankLine(int numLines)
		{
			for (int i = 0; i < numLines; i++)
				Console.WriteLine("");
		}

		public void Log(LogType type, LogCategory category, string message)
		{
#if !DEBUG
			if (type == LogType.Debug)
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
	}
}
