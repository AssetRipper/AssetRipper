using AssetRipper.Utils;
using System;
using System.Collections.Generic;

namespace AssetRipper.Logging
{
	public static class Logger
	{
		private static readonly List<ILogger> loggers = new List<ILogger>();

		static Logger()
		{
			Cpp2IL.Core.Logger.InfoOverride = new Action<string, string>((string message, string source) => Log(LogType.Info, LogCategory.Cpp2IL, message));
			Cpp2IL.Core.Logger.WarnOverride = new Action<string, string>((string message, string source) => Log(LogType.Warning, LogCategory.Cpp2IL, message));
			Cpp2IL.Core.Logger.ErrorOverride = new Action<string, string>((string message, string source) => Log(LogType.Error, LogCategory.Cpp2IL, message));
			Cpp2IL.Core.Logger.VerboseOverride = new Action<string, string>((string message, string source) => Log(LogType.Debug, LogCategory.Cpp2IL, message));
		}

		public static void Log(LogType type, LogCategory category, string message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			foreach (ILogger instance in loggers)
				instance?.Log(type, category, message);
		}

		public static void Log(LogType type, LogCategory category, string[] messages)
		{
			if (messages == null) throw new ArgumentNullException(nameof(messages));
			foreach (string message in messages)
				Log(type, category, message);
		}

		public static void BlankLine() => BlankLine(1);

		public static void BlankLine(int numLines)
		{
			foreach (ILogger instance in loggers)
				instance?.BlankLine(numLines);
		}

		private static void LogReleaseInformation()
		{
#if VIRTUAL
			Log(LogType.Info, LogCategory.General, "Build type: Virtual");
#elif DEBUG
			Log(LogType.Info, LogCategory.General, "Build type: Debug");
#else
			Log(LogType.Info, LogCategory.General, "Build type: Release");
#endif
		}

		public static void LogSystemInformation()
		{
			Log(LogType.Info, LogCategory.General, $"Operating System: {RunetimeUtils.RuntimeOS.ToString()}");
			LogReleaseInformation();
		}

		public static void Add(ILogger logger) => loggers.Add(logger);

		public static void Remove(ILogger logger) => loggers.Remove(logger);

		public static void Clear() => loggers.Clear();
	}
}
