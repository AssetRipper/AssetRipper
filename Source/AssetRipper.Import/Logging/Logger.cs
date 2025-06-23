using System.Text;

namespace AssetRipper.Import.Logging;

public static class Logger
{
	private static readonly object _lock = new();
	private static readonly List<ILogger> loggers = new();
	public static bool AllowVerbose { get; set; }

	public static event Action<string, object?> OnStatusChanged = (_, _) => { };

	static Logger()
	{
		Cpp2IL.Core.Logging.Logger.InfoLog += (message, source) => LogCpp2IL(LogType.Info, message);
		Cpp2IL.Core.Logging.Logger.WarningLog += (message, source) => LogCpp2IL(LogType.Verbose, message);
		Cpp2IL.Core.Logging.Logger.ErrorLog += (message, source) => LogCpp2IL(LogType.Error, message);
		Cpp2IL.Core.Logging.Logger.VerboseLog += (message, source) => LogCpp2IL(LogType.Verbose, message);
	}

	private static void LogCpp2IL(LogType logType, string message)
	{
		Log(logType, LogCategory.Cpp2IL, message.Trim());
	}

	public static void Log(LogType type, LogCategory category, string message)
	{
		if (AssetRipperRuntimeInformation.Build.Debug && type == LogType.Debug)
		{
			return;
		}

		if (type == LogType.Verbose && !AllowVerbose)
		{
			return;
		}

		ArgumentNullException.ThrowIfNull(message);

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
		ArgumentNullException.ThrowIfNull(messages);

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

	private static void ErrorIfBigEndian()
	{
		if (!BitConverter.IsLittleEndian)
		{
			Error("Big Endian processors are not supported!");
		}
	}

	public static void LogSystemInformation(string programName)
	{
		Log(LogType.Info, LogCategory.System, programName);
		Log(LogType.Info, LogCategory.System, $"System Version: {AssetRipperRuntimeInformation.OS.Version}");
		Log(LogType.Info, LogCategory.System, $"Operating System: {AssetRipperRuntimeInformation.OS.Name} {AssetRipperRuntimeInformation.ProcessArchitecture}");
		ErrorIfBigEndian();
		Log(LogType.Info, LogCategory.System, $"AssetRipper Version: {AssetRipperRuntimeInformation.Build.Version}");
		Log(LogType.Info, LogCategory.System, $"AssetRipper Build Type: {AssetRipperRuntimeInformation.Build.Configuration} {AssetRipperRuntimeInformation.Build.Type}");
		Log(LogType.Info, LogCategory.System, $"UTC Current Time: {AssetRipperRuntimeInformation.CurrentTime}");
		Log(LogType.Info, LogCategory.System, $"UTC Compile Time: {AssetRipperRuntimeInformation.CompileTime}");
	}

	public static void Add(ILogger logger) => loggers.Add(logger);

	public static void Remove(ILogger logger) => loggers.Remove(logger);

	public static void Clear() => loggers.Clear();

	public static void SendStatusChange(string newStatus, object? context = null) => OnStatusChanged(newStatus, context);
}
