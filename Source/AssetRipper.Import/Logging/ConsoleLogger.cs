namespace AssetRipper.Import.Logging;

public class ConsoleLogger : ILogger
{
	public ConsoleLogger() : this(false) { }
	/// <param name="resizeConsole">If true, on Windows it will resize the console to 80% of the maximum size.</param>
	public ConsoleLogger(bool resizeConsole)
	{
		if (resizeConsole && OperatingSystem.IsWindows())
		{
			try
			{
				Console.WindowWidth = (int)(Console.LargestWindowWidth * 0.8f);
				Console.WindowHeight = (int)(Console.LargestWindowHeight * 0.8f);
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
		{
			Console.WriteLine();
		}
	}

	public void Log(LogType type, LogCategory category, string message)
	{
		if (type == LogType.Info)
		{
			if (category == LogCategory.None)
			{
				Console.WriteLine(message);
			}
			else
			{
				Console.WriteLine($"{category} : {message}");
			}

			return;
		}

		ConsoleColor foreColor = Console.ForegroundColor;

		switch (type)
		{
			case LogType.Debug:
				Console.ForegroundColor = ConsoleColor.DarkBlue;
				break;

			case LogType.Verbose:
				Console.ForegroundColor = ConsoleColor.DarkGray;
				break;

			case LogType.Warning:
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				break;

			case LogType.Error:
				Console.ForegroundColor = ConsoleColor.DarkRed;
				break;
		}

		if (category == LogCategory.None)
		{
			Console.WriteLine(message);
		}
		else
		{
			Console.WriteLine($"{category} : {message}");
		}

		Console.ForegroundColor = foreColor;
	}
}
