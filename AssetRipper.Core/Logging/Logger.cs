using AssetRipper.Core.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace AssetRipper.Core.Logging
{
	public static class Logger
	{
		private static readonly object _lock = new();
		private static readonly List<ILogger> loggers = new();
		public static bool AllowVerbose { get; set; }

		public static event Action<string, object?> OnStatusChanged = (_, _) => { };

		static Logger()
		{
			Cpp2IL.Core.Logger.InfoLog += (string message, string source) => LogCpp2IL(LogType.Info, message);
			Cpp2IL.Core.Logger.WarningLog += (string message, string source) => LogCpp2IL(LogType.Verbose, message);
			Cpp2IL.Core.Logger.ErrorLog += (string message, string source) => LogCpp2IL(LogType.Error, message);
			Cpp2IL.Core.Logger.VerboseLog += (string message, string source) => LogCpp2IL(LogType.Verbose, message);
		}

		private static void LogCpp2IL(LogType logType, string message)
		{
			Log(logType, LogCategory.Cpp2IL, message.Trim());
		}

		public static void Log(LogType type, LogCategory category, string message)
		{
#if !DEBUG
			if (type == LogType.Debug)
			{
				return;
			}
#endif
			if (type == LogType.Verbose && !AllowVerbose)
			{
				return;
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			lock (_lock)
			{
				foreach (ILogger instance in loggers)
				{
					instance?.Log(type, category, message);
				}
			}
		}

		public static void Log(LogType type, LogCategory category, string[] messages)
		{
			if (messages == null)
			{
				throw new ArgumentNullException(nameof(messages));
			}

			foreach (string message in messages)
			{
				Log(type, category, message);
			}
		}

		public static void BlankLine() => BlankLine(1);
		public static void BlankLine(int numLines)
		{
			foreach (ILogger instance in loggers)
			{
				instance?.BlankLine(numLines);
			}
		}

		public static void Info(string message) => Log(LogType.Info, LogCategory.None, message);
		public static void Info(LogCategory category, string message) => Log(LogType.Info, category, message);
		public static void Warning(string message) => Log(LogType.Warning, LogCategory.None, message);
		public static void Warning(LogCategory category, string message) => Log(LogType.Warning, category, message);
		public static void Error(string message) => Log(LogType.Error, LogCategory.None, message);
		public static void Error(LogCategory category, string message) => Log(LogType.Error, category, message);
		public static void Error(Exception e) => Error(LogCategory.None, null, e);
		public static void Error(string message, Exception e) => Error(LogCategory.None, message, e);
		public static void Error(LogCategory category, string? message, Exception e)
		{
			StringBuilder sb = new();
			if (message != null)
			{
				sb.AppendLine(message);
			}

			sb.AppendLine(e.ToString());
			Log(LogType.Error, category, sb.ToString());
		}
		public static void Verbose(string message) => Log(LogType.Verbose, LogCategory.None, message);
		public static void Verbose(LogCategory category, string message) => Log(LogType.Verbose, category, message);
		public static void Debug(string message) => Log(LogType.Debug, LogCategory.None, message);
		public static void Debug(LogCategory category, string message) => Log(LogType.Debug, category, message);

		private static void LogReleaseInformation()
		{
#if DEBUG
			Log(LogType.Info, LogCategory.System, $"AssetRipper Build Type: Debug {GetBuildType()}");
#else
			Log(LogType.Info, LogCategory.System, $"AssetRipper Build Type: Release {GetBuildType()}");
#endif
		}

		private static string GetBuildType()
		{
			return File.Exists(ExecutingDirectory.Combine("AssetRipper.Core.dll")) ? "Compiled" : "Published";
		}

		private static void LogOperatingSystemInformation()
		{
			Log(LogType.Info, LogCategory.System, $"System Version: {Environment.OSVersion.VersionString}");
			string architecture = Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit";
			Log(LogType.Info, LogCategory.System, $"Operating System: {GetOsName()} {architecture}");
		}

		public static void LogSystemInformation(string programName)
		{
			Log(LogType.Info, LogCategory.System, programName);
			LogOperatingSystemInformation();
			Log(LogType.Info, LogCategory.System, $"AssetRipper Version: {BuildInfo.Version}");
			LogReleaseInformation();
			Log(LogType.Info, LogCategory.System, $"UTC Current Time: {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}");
			Log(LogType.Info, LogCategory.System, $"UTC Compile Time: {GetCompileTime()}");
		}

		private static string GetCompileTime()
		{
			string path = ExecutingDirectory.Combine("compile_time.txt");
			if (File.Exists(path))
			{
				return File.ReadAllText(path).Trim();
			}
			else
			{
				return "Unknown";
			}
		}

		private static string GetOsName()
		{
			if (OperatingSystem.IsWindows())
			{
				return "Windows";
			}
			else if (OperatingSystem.IsLinux())
			{
				return "Linux";
			}
			else if (OperatingSystem.IsMacOS())
			{
				return "MacOS";
			}
			else if (OperatingSystem.IsBrowser())
			{
				return "Browser";
			}
			else if (OperatingSystem.IsAndroid())
			{
				return "Android";
			}
			else if (OperatingSystem.IsIOS())
			{
				return "iOS";
			}
			else if (OperatingSystem.IsFreeBSD())
			{
				return "FreeBSD";
			}
			else
			{
				return "Other";
			}
		}

		public static void Add(ILogger logger) => loggers.Add(logger);

		public static void Remove(ILogger logger) => loggers.Remove(logger);

		public static void Clear() => loggers.Clear();

		public static void SendStatusChange(string newStatus, object? context = null) => OnStatusChanged(newStatus, context);
	}
}
