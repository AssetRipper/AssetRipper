using System.Collections.Generic;

namespace AssetRipper
{
	public static class Logger
	{
		private static readonly List<ILogger> loggers = new List<ILogger>();

		public static void Log(LogType type, LogCategory category, string message)
		{
			foreach (ILogger instance in loggers)
				instance?.Log(type, category, message);
		}

		public static void Add(ILogger logger) => loggers.Add(logger);

		public static void Remove(ILogger logger) => loggers.Remove(logger);

		public static void Clear() => loggers.Clear();
	}
}
